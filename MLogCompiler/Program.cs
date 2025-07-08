using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MLogCompiler.Compiler;

//if (args.Length == 0)
//{
//    Console.Error.WriteLine("Usage: mlogc <source.cs>");
//    Environment.Exit(1);
//}

var source = File.ReadAllText("Example.cs");
var tree = CSharpSyntaxTree.ParseText(source);
var comp = CSharpCompilation.Create("src",
    new[] { tree },
    new[]
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Logic).Assembly.Location)
    },
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

var model = comp.GetSemanticModel(tree);
var tr = new Translator(model);
tr.Visit(tree.GetRoot());

var emitter = new MlogEmitter(tr.Instr);
emitter.Emit(Console.Out);