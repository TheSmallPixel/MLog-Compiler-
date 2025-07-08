using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLogCompiler.Compiler
{
    internal sealed record Ir(string Mn, params string[] Ops)
    {
        public static Ir Label(string l) => new("label", l);
        public static Ir Jump(string l, string cond, string a, string b) => new("jump", l, cond, a, b);
        public static Ir Op(string op, string dest, string a, string b) => new("op", op, dest, a, b);
        public static Ir Set(string v, string val) => new("set", v, val);
        public static Ir Custom(string instr, params string[] ops)
            => new("custom", (new[] { instr }).Concat(ops).ToArray());
    }
}
