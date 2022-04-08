namespace SmartTool
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.Text;

    internal class Compiler
    {
        public string Compile(string filepath)
        {
            Console.WriteLine($"Starting compilation of: '{filepath}'");

            var sourceCode = File.ReadAllText(filepath);

            using(var peStream = new MemoryStream())
            {
                var result = GenerateCode(sourceCode);

                if(!result.EmitResult.Success)
                {
                    Console.WriteLine("Compilation done with error.");

                    var failures = result.EmitResult.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error).ToList();

                    foreach(var diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    return null;
                }

                Console.WriteLine("Compilation done without any error.");

                //peStream.Seek(0, SeekOrigin.Begin);

                //return peStream.ToArray();
                return result.OutputPath;
            }
        }

        private static GenerateCodeResponse GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, CSharpParseOptions.Default);
            var sysIOFrameworkPath = typeof(Directory).GetTypeInfo().Assembly.Location;
            var frameworkPath = Directory.GetParent(sysIOFrameworkPath);
            var assemblyLocation = Assembly.GetEntryAssembly().Location;
            var projectPath = Directory.GetParent(assemblyLocation);
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile($"{projectPath}\\SmartTool.dll"),
                MetadataReference.CreateFromFile($"{projectPath}\\Stratis.SmartContracts.dll"),
                MetadataReference.CreateFromFile($"{projectPath}\\Iot.Device.Bindings.dll"),
                MetadataReference.CreateFromFile($"{projectPath}\\System.ComponentModel.Annotations.dll"),
                MetadataReference.CreateFromFile($"{projectPath}\\System.Linq.Async.dll"),
                MetadataReference.CreateFromFile($"{projectPath}\\UnitsNet.dll"),
                MetadataReference.CreateFromFile(frameworkPath.FullName + "\\System.Runtime.dll"),
                MetadataReference.CreateFromFile(frameworkPath.FullName + "\\mscorlib.dll"),
                MetadataReference.CreateFromFile(frameworkPath.FullName + "\\netstandard.dll")
            };
            var outputPath = $"{projectPath}\\Generated.dll";
            var compilation = CSharpCompilation.Create("Generated.dll")
                .AddSyntaxTrees(new[] { parsedSyntaxTree })
                .AddReferences(references)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var emitResult = compilation.Emit(outputPath);
            return new GenerateCodeResponse() { OutputPath = outputPath, EmitResult = emitResult};
        }
    }

    class GenerateCodeResponse
    {
        public string OutputPath { get; set; }
        public EmitResult EmitResult { get; set; }
    }
}
