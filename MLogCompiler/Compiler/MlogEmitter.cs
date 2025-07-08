using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLogCompiler.Compiler
{
    internal sealed class MlogEmitter
    {
        private readonly IReadOnlyList<Ir> _ir;
        public MlogEmitter(IReadOnlyList<Ir> ir) => _ir = ir;
        public void Emit(TextWriter w)
        {
            foreach (var i in _ir)
            {
                switch (i.Mn)
                {
                    case "label": w.WriteLine($"{i.Ops[0]}:"); break;
                    case "jump": w.WriteLine($"jump {i.Ops[0]} {i.Ops[1]} {i.Ops[2]} {i.Ops[3]}"); break;
                    case "op": w.WriteLine($"op {i.Ops[0]} {i.Ops[1]} {i.Ops[2]} {i.Ops[3]}"); break;
                    case "set": w.WriteLine($"set {i.Ops[0]} {i.Ops[1]}"); break;
                    case "custom": EmitCustom(w, i.Ops); break;
                }
            }
        }
        private void EmitCustom(TextWriter w, string[] o)
        {
            var c = o[0];
            switch (c)
            {
                case "ubind": w.WriteLine($"ubind {o[1]}"); break;
                case "ucontrol": w.WriteLine($"ucontrol {string.Join(" ", o.Skip(1))}"); break;
                case "ulocate": w.WriteLine($"ulocate {o[1]} {o[2]} {string.Join(" ", o.Skip(3))}"); break;
                case "uradar": w.WriteLine($"uradar {o[1]} {string.Join(" ", o.Skip(2))}"); break;
                case "sensor": w.WriteLine($"sensor {o[1]} {o[2]} {o[3]}"); break;
                case "control": w.WriteLine($"control {string.Join(" ", o.Skip(1))}"); break;
                case "print": w.WriteLine($"print {o[1]}"); break;
                case "printflush": w.WriteLine($"printflush {o[1]}"); break;
                case "draw": w.WriteLine($"draw {string.Join(" ", o.Skip(1))}"); break;
                case "drawflush": w.WriteLine($"drawflush {o[1]}"); break;
                case "wait": w.WriteLine($"wait {o[1]}"); break;
                case "write": w.WriteLine($"write {o[1]} {o[2]} {o[3]}"); break;
                case "read": w.WriteLine($"read {o[1]} {o[2]} {o[3]}"); break;
                case "getlink": w.WriteLine($"getlink {o[1]} {o[2]}"); break;
                default: throw new Exception($"custom {c} not mapped");
            }
        }

    }
}
