using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace MLogCompiler.Compiler
{
    internal sealed class Translator : CSharpSyntaxWalker
    {
        private readonly SemanticModel _m;
        private readonly List<Ir> _ir = new();
        private int _tmp = 0, _lbl = 0;

        public IReadOnlyList<Ir> Instr => _ir;

        public Translator(SemanticModel m) : base(SyntaxWalkerDepth.StructuredTrivia)
            => _m = m;

        private string Tmp() => $"tmp{_tmp++}";
        private string Lbl(string p = "L") => $"{p}{_lbl++}";

        // ---------------- Statements ----------------
        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax n)
        {
            foreach (var v in n.Declaration.Variables)
            {
                var name = v.Identifier.Text;
                var rhs = v.Initializer != null ? Expr(v.Initializer.Value) : "0";
                _ir.Add(Ir.Set(name, rhs));
            }
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax n)
        {
            if (n.Expression is AssignmentExpressionSyntax a)
                Assign(a);
            else
                _ = Expr(n.Expression);   // side‑effect invocation
        }

        private void Assign(AssignmentExpressionSyntax a)
        {
            var left = ((IdentifierNameSyntax)a.Left).Identifier.Text;
            var rhs = Expr(a.Right);
            _ir.Add(Ir.Set(left, rhs));
        }

        public override void VisitIfStatement(IfStatementSyntax n)
        {
            var lblElse = Lbl("else");
            var lblEnd = Lbl("endif");
            var cond = Expr(n.Condition);
            _ir.Add(Ir.Jump(lblElse, "equal", cond, "false"));
            Visit(n.Statement);
            _ir.Add(Ir.Jump(lblEnd, "always", "0", "0"));
            _ir.Add(Ir.Label(lblElse));
            if (n.Else != null) Visit(n.Else.Statement);
            _ir.Add(Ir.Label(lblEnd));
        }

        public override void VisitWhileStatement(WhileStatementSyntax n)
        {
            var lblStart = Lbl("whileStart");
            var lblEnd = Lbl("whileEnd");
            _ir.Add(Ir.Label(lblStart));
            var cond = Expr(n.Condition);
            _ir.Add(Ir.Jump(lblEnd, "equal", cond, "false"));
            Visit(n.Statement);
            _ir.Add(Ir.Jump(lblStart, "always", "0", "0"));
            _ir.Add(Ir.Label(lblEnd));
        }

        public override void VisitForStatement(ForStatementSyntax n)
        {
            foreach (var v in n.Declaration.Variables)
            {
                var lhs = v.Identifier.Text;
                var rhs = v.Initializer != null ? Expr(v.Initializer.Value) : "0";
                _ir.Add(Ir.Set(lhs, rhs));
            }
            var lblStart = Lbl("forStart");
            var lblEnd = Lbl("forEnd");
            _ir.Add(Ir.Label(lblStart));
            var cond = n.Condition != null ? Expr(n.Condition) : "true";
            _ir.Add(Ir.Jump(lblEnd, "equal", cond, "false"));
            Visit(n.Statement);
            foreach (var inc in n.Incrementors)
            {
                if (inc is PostfixUnaryExpressionSyntax p && p.OperatorToken.IsKind(SyntaxKind.PlusPlusToken))
                {
                    var id = ((IdentifierNameSyntax)p.Operand).Identifier.Text;
                    _ir.Add(Ir.Op("add", id, id, "1"));
                }
            }
            _ir.Add(Ir.Jump(lblStart, "always", "0", "0"));
            _ir.Add(Ir.Label(lblEnd));
        }

        // ---------------- Expressions ----------------
        private string Expr(ExpressionSyntax e)
        {
            return e switch
            {
                LiteralExpressionSyntax lit => Lit(lit),
                IdentifierNameSyntax id => id.Identifier.Text,
                BinaryExpressionSyntax bin => Bin(bin),
                ParenthesizedExpressionSyntax p => Expr(p.Expression),
                ConditionalExpressionSyntax ternary => Ternary(ternary),
                InvocationExpressionSyntax inv => Invoke(inv),
                _ => throw new NotSupportedException($"Expression not supported: {e.Kind()}")
            };
        }

        private string Lit(LiteralExpressionSyntax lit)
        {
            if (lit.IsKind(SyntaxKind.NumericLiteralExpression))
                return lit.Token.ValueText;
            if (lit.IsKind(SyntaxKind.TrueLiteralExpression)) return "true";
            if (lit.IsKind(SyntaxKind.FalseLiteralExpression)) return "false";
            if (lit.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var txt = lit.Token.ValueText;
                return txt.StartsWith("@") ? txt : $"\"{txt}\"";
            }
            throw new NotSupportedException("Literal kind not handled");
        }

        private string Bin(BinaryExpressionSyntax bin)
        {
            var a = Expr(bin.Left);
            var b = Expr(bin.Right);
            var t = Tmp();
            var op = bin.OperatorToken.Kind() switch
            {
                SyntaxKind.PlusToken => "add",
                SyntaxKind.MinusToken => "sub",
                SyntaxKind.AsteriskToken => "mul",
                SyntaxKind.SlashToken => "div",
                SyntaxKind.PercentToken => "mod",
                SyntaxKind.EqualsEqualsToken => "equal",
                SyntaxKind.ExclamationEqualsToken => "notEqual",
                SyntaxKind.LessThanToken => "lessThan",
                SyntaxKind.LessThanEqualsToken => "lessThanEq",
                SyntaxKind.GreaterThanToken => "greaterThan",
                SyntaxKind.GreaterThanEqualsToken => "greaterThanEq",
                _ => throw new NotSupportedException("Operator not handled")
            };
            _ir.Add(Ir.Op(op, t, a, b));
            return t;
        }

        private string Ternary(ConditionalExpressionSyntax c)
        {
            var res = Tmp();
            var lblF = Lbl("ternF");
            var lblE = Lbl("ternE");
            var cond = Expr(c.Condition);
            _ir.Add(Ir.Jump(lblF, "equal", cond, "false"));
            var tVal = Expr(c.WhenTrue);
            _ir.Add(Ir.Set(res, tVal));
            _ir.Add(Ir.Jump(lblE, "always", "0", "0"));
            _ir.Add(Ir.Label(lblF));
            var fVal = Expr(c.WhenFalse);
            _ir.Add(Ir.Set(res, fVal));
            _ir.Add(Ir.Label(lblE));
            return res;
        }

        // ------------- Invocation handling -------------
        private string Invoke(InvocationExpressionSyntax call)
        {
            // Get container + method via semantic model (but fall back to syntax chains)
            var info = _m.GetSymbolInfo(call);
            var msym = info.Symbol as IMethodSymbol;
            var container = msym?.ContainingType?.ToDisplayString() ?? (call.Expression as MemberAccessExpressionSyntax)?.Expression.ToString();
            var mname = msym?.Name ?? (call.Expression as MemberAccessExpressionSyntax)?.Name.Identifier.Text;

            // --- System.Math ---------------------------------
            if (container is "System.Math" or "Math")
            {
                var mop = mname switch { "Sin" => "sin", "Cos" => "cos", "Tan" => "tan", "Sqrt" => "sqrt", "Abs" => "abs", "Max" => "max", "Min" => "min", _ => null };
                if (mop != null)
                {
                    var a = Expr(call.ArgumentList.Arguments[0].Expression);
                    var b = call.ArgumentList.Arguments.Count > 1 ? Expr(call.ArgumentList.Arguments[1].Expression) : "0";
                    var t = Tmp();
                    _ir.Add(Ir.Op(mop, t, a, b));
                    return t;
                }
            }

            // --- Logic hierarchy -----------------------------
            switch (container)
            {
                case "Logic": return HandleLogicRoot(mname, call);
                case "Logic.Logic.Block": return HandleBlock(mname, call);
                case "Block.Control": return HandleBlockControl(mname, call);
                case "Logic.IO": return HandleIO(mname, call);
                case "Logic.IO.Draw": return HandleDraw(mname, call);
                case "Logic.Unit": return HandleUnit(mname, call);
                case "Logic.Unit.Locate": return HandleLocate(mname, call);
                case "Logic.Unit.Radar": return HandleRadar(mname, call);
                case "Logic.Unit.Control": return HandleUnitControl(mname, call);
            }

            throw new NotSupportedException($"call {container}.{mname} not mapped");
        }

        // ---------- Logic root (Bind/Wait/Print etc.) --------------
        private string HandleLogicRoot(string name, InvocationExpressionSyntax call)
        {
            switch (name)
            {
                case "Bind":
                    var tok = EnumArgToToken(call.ArgumentList.Arguments[0], "@");
                    _ir.Add(Ir.Custom("ubind", tok)); return "null";
                case "UnitBind":
                    _ir.Add(Ir.Custom("ubind", Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "UnitMoveTo":
                    _ir.Add(Ir.Custom("ucontrol", "move", Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression), "0", "0", "0")); return "null";
                case "UnBind":
                    _ir.Add(Ir.Custom("ubind", "null")); return "null";
                case "Wait":
                    _ir.Add(Ir.Custom("wait", Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "Print":
                    _ir.Add(Ir.Custom("print", Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "PrintFlush":
                    _ir.Add(Ir.Custom("printflush", Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
            }
            throw new NotSupportedException(name);
        }

        // ---------- Block.* ---------------------------------------
        private string HandleBlock(string name, InvocationExpressionSyntax call)
        {
            switch (name)
            {
                case "Sensor":
                    _ir.Add(Ir.Custom("sensor", Expr(call.ArgumentList.Arguments[0].Expression), EnumArgToToken(call.ArgumentList.Arguments[1], ""), call.ArgumentList.Arguments[2].Expression.ToString()));
                    return Expr(call.ArgumentList.Arguments[0].Expression);
                case "SensorItem":
                    var itemTok = EnumArgToToken(call.ArgumentList.Arguments[2], "@item");
                    _ir.Add(Ir.Custom("sensor", Expr(call.ArgumentList.Arguments[0].Expression), EnumArgToToken(call.ArgumentList.Arguments[1], ""), itemTok));
                    return Expr(call.ArgumentList.Arguments[0].Expression);
                case "SensorValue":
                    _ir.Add(Ir.Custom("sensor", Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression)));
                    return Expr(call.ArgumentList.Arguments[0].Expression);
                case "GetLink":
                    _ir.Add(Ir.Custom("getlink", Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression)));
                    return Expr(call.ArgumentList.Arguments[0].Expression);
                case "PrintFlush":
                    _ir.Add(Ir.Custom("printflush", EnumArgToToken(call.ArgumentList.Arguments[0], ""))); return "null";
                case "DrawFlush":
                    _ir.Add(Ir.Custom("drawflush", EnumArgToToken(call.ArgumentList.Arguments[0], ""))); return "null";
            }
            throw new NotSupportedException(name);
        }

        private string HandleBlockControl(string name, InvocationExpressionSyntax call)
        {
            var blk = EnumArgToToken(call.ArgumentList.Arguments[0], "");
            switch (name)
            {
                case "Enable":
                    _ir.Add(Ir.Custom("control", "enabled", blk, Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "Color":
                    _ir.Add(Ir.Custom("control", "color", blk, Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "Shoot":
                    _ir.Add(Ir.Custom("control", "shoot", blk, Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "ShootId":
                    _ir.Add(Ir.Custom("control", "shootp", blk, Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "Config":
                    var extras = call.ArgumentList.Arguments.Skip(1).Select(a => Expr(a.Expression)).ToArray();
                    _ir.Add(Ir.Custom("control", (new[] { "config", blk }).Concat(extras).ToArray())); return "null";
            }
            throw new NotSupportedException(name);
        }

        // ---------- IO / Draw -------------------------------------
        private string HandleIO(string name, InvocationExpressionSyntax call)
        {
            switch (name)
            {
                case "Print": _ir.Add(Ir.Custom("print", Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "Write": _ir.Add(Ir.Custom("write", Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression))); return "null";
                case "Read": _ir.Add(Ir.Custom("read", Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression), Expr(call.ArgumentList.Arguments[0].Expression))); return Expr(call.ArgumentList.Arguments[0].Expression);
            }
            throw new NotSupportedException(name);
        }
        private string HandleDraw(string name, InvocationExpressionSyntax call)
        {
            var args = call.ArgumentList.Arguments.Select(a => Expr(a.Expression)).ToArray();
            _ir.Add(Ir.Custom("draw", (new[] { name.ToLower() }).Concat(args).ToArray()));
            return "null";
        }

        // ---------- Unit.* ---------------------------------------
        private string HandleUnit(string name, InvocationExpressionSyntax call)
        {

            if (name == "Bind")
            {
                // Enum UnitType → @unit token
                var tok = EnumArgToToken(call.ArgumentList.Arguments[0], "@");
                _ir.Add(Ir.Custom("ubind", tok));
                return "null";
            }
            if (name == "UnBind") { _ir.Add(Ir.Custom("ubind", "null")); return "null"; }
            throw new NotSupportedException(name);
        }
        private string HandleLocate(string name, InvocationExpressionSyntax call)
        {
            string mode = name.ToLower();
            var args = new List<string>();
            if (mode == "ore")
            {
                args.Add(EnumArgToToken(call.ArgumentList.Arguments[0], ""));
                args.AddRange(ConvertSortArgs(call.ArgumentList.Arguments[1], call.ArgumentList.Arguments[2]));
                args.AddRange(PosRadius(call, 3));
            }
            else if (mode == "building")
            {
                args.Add(EnumArgToToken(call.ArgumentList.Arguments[0], ""));
                args.AddRange(ConvertSortArgs(call.ArgumentList.Arguments[1], call.ArgumentList.Arguments[2]));
                args.AddRange(PosRadius(call, 3));
            }
            else if (mode == "spawn") { }
            else if (mode == "damaged")
            {
                args.AddRange(ConvertSortArgs(call.ArgumentList.Arguments[0], call.ArgumentList.Arguments[1]));
                args.AddRange(PosRadius(call, 2));
            }
            var res = "locRes" + Tmp();
            _ir.Add(Ir.Custom("ulocate", (new[] { mode, res }).Concat(args).ToArray()));
            return res; 
        }
        private string HandleRadar(string name, InvocationExpressionSyntax call)
        {
            var res = "rad" + Tmp();
            var args = new List<string> { EnumArgToToken(call.ArgumentList.Arguments[0], "") };
            args.AddRange(ConvertSortArgs(call.ArgumentList.Arguments[1], call.ArgumentList.Arguments[2]));
            args.AddRange(PosRadius(call, 3));
            _ir.Add(Ir.Custom("uradar", (new[] { res }).Concat(args).ToArray()));
            return res; 
        }
        private string HandleUnitControl(string name, InvocationExpressionSyntax call)
        {
            var args = call.ArgumentList.Arguments.Select(a => Expr(a.Expression)).ToList();
            string action = name.ToLower() switch
            {
                "idle" => "idle",
                "stop" => "stop",
                "move" => "move",
                "approach" => "approach",
                "pathfind" => "pathfind",
                "autopathfind" => "pathfind",
                "boost" => "boost",
                "target" => "shoot",
                "targettp" => "shootp",
                "itemdrop" => "itemDrop",
                "itemtake" => "itemTake",
                "payloaddrop" => "payloaddrop",
                "payloadtake" => "payloadtake",
                "payloadenter" => "payloadenter",
                "mine" => "mine",
                "flag" => "flag",
                "build" => "build",
                "getblock" => "getblock",
                "within" => "within",
                _ => null
            };
            if (action == null) throw new NotSupportedException(name);
            _ir.Add(Ir.Custom("ucontrol", (new[] { action }).Concat(args).ToArray()));
            return "null";
        }

        // ── helpers ─────────────────────────────────────────────
        private string EnumArgToToken(ArgumentSyntax arg, string prefix)
        {
            if (arg.Expression is MemberAccessExpressionSyntax mae)
                return prefix + mae.Name.Identifier.Text.ToLower();
            return Expr(arg.Expression);
        }
        private string[] ConvertSortArgs(ArgumentSyntax sortArg, ArgumentSyntax orderArg)
        {
            string sort = EnumArgToToken(sortArg, ""), order = EnumArgToToken(orderArg, "");
            return new[] { sort, order };
        }
        private IEnumerable<string> PosRadius(InvocationExpressionSyntax call, int startIdx)
        {
            for (int i = startIdx; i < call.ArgumentList.Arguments.Count; i++)
                yield return Expr(call.ArgumentList.Arguments[i].Expression);
        }
    }
}
