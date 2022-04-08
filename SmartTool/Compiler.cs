namespace SmartTool
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;

    internal class Compiler
    {
        public byte[] Compile(string filepath)
        {
            Console.WriteLine($"Starting compilation of: '{filepath}'");

            var sourceCode = File.ReadAllText(filepath);

            using(var peStream = new MemoryStream())
            {
                var result = GenerateCode(sourceCode).Emit(peStream);

                if(!result.Success)
                {
                    Console.WriteLine("Compilation done with error.");

                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error).ToList();

                    foreach(var diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    return null;
                }

                Console.WriteLine("Compilation done without any error.");

                peStream.Seek(0, SeekOrigin.Begin);

                return peStream.ToArray();
            }
        }

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, CSharpParseOptions.Default);
            var clrLibAssemblyLocation = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var frameworkPath = Directory.GetParent(clrLibAssemblyLocation);
            var entryAssemblyLocation = Assembly.GetEntryAssembly().Location;
            var projectPath = Directory.GetParent(entryAssemblyLocation);
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
                MetadataReference.CreateFromFile(frameworkPath.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll"),
                MetadataReference.CreateFromFile(frameworkPath.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(frameworkPath.FullName + Path.DirectorySeparatorChar + "netstandard.dll")
            };

            var compilation = CSharpCompilation.Create("Generated.dll")
                .AddSyntaxTrees(new[] { parsedSyntaxTree })
                .AddReferences(references)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.Emit($"{projectPath}\\Generated.dll");
            return compilation;
        }
    }
}
