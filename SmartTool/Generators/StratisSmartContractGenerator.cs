using NBitcoin;
using SmartTool.Generators.Interfaces;
using Stratis.SmartContracts.CLR.Compilation;
using Stratis.SmartContracts.Core.Hashing;
using Stratis.SmartContracts.Tools.Sct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartTool.Generators
{
    public class StratisSmartContractGenerator : ISmartContractGenerator
    {

        private string outputPath;
        private Type program;
        public void GenerateSmartContract(Type program, string outputPath)
        {
            this.program = program;
            this.outputPath = outputPath;

            // Retrieval of fields and methods from the program passed to the tool
            var smartContractFields = program.GetFieldsByAttribute(nameof(SmartContractAttribute));
            var smartContractMethods = program.GetMethodsByAttribute(nameof(SmartContractAttribute));

            var constructorCode = string.Empty;

            // Mapping of fields to stratis 
            var fieldsCode = string.Join(Environment.NewLine, smartContractFields.Select(f =>
            {
                return
                    $@"
                        public {f.FieldType.FullName} {f.Name} {{
	                        get => this.PersistentState.Get{(f.FieldType.BaseType?.Name == "Array" ? $"Array<{f.FieldType.FullName.Replace("[]", "")}>" : f.FieldType.Name)}(nameof(this.{f.Name}));
	                        private set => this.PersistentState.Set{(f.FieldType.BaseType?.Name == "Array" ? "Array" : f.FieldType.Name)}(nameof(this.{f.Name}), value);
                        }}";
            }).ToArray());

            // Methods code retrieval
            var methodsCode = string.Join(Environment.NewLine, smartContractMethods.Select(method =>
            {

                // Decompiles the code of the current method
                string methodCode = program.Decompile(method.MetadataToken);

                // Removes attribute tags
                foreach (var att in method.CustomAttributes)
                {
                    var name = att.AttributeType.Name.Replace("Attribute", "");
                    methodCode = methodCode.Replace($"[{name}]", string.Empty);
                }

                return methodCode;
            }));

            var fileName = "StratisSmartContract";
            var directory = $"{outputPath}/{program.Name}/";
            var smartContract = @$"
                using System;
                using Stratis.SmartContracts;

                // The [Deploy] attribute only needs to be specified when more than one class is declared in the file, but specifying it anyway is fine.
                [Deploy]

                public class {fileName} : SmartContract
                {{

                        //Constructor
                        public {fileName}(ISmartContractState smartContractState): base(smartContractState)
                        {{
                            {constructorCode}
                        }}

                        {fieldsCode}

                        {methodsCode}
                    }}
            ";

            // Project references
            var projectReferences = new List<ProjectReference>();
            projectReferences.Add(new ProjectReference() { Name = "Stratis.SmartContracts", Version = "1.2.1" });

            // Csproj code generation
            var csprojCode = CsprojGenerator.GenerateCsproj(fileName, projectReferences, OutputType.ClassLibrary);

            // Fix usings
            smartContract = smartContract.FixUsings();

            // Fix auto imported using
            smartContract = smartContract.Replace("using SmartTool;", "");

            if (!Directory.Exists($"{directory}{fileName}"))
            {
                Directory.CreateDirectory($"{directory}{fileName}");
            }

            // Create files
            File.WriteAllText($"{directory}{fileName}/{fileName}.csproj", csprojCode);
            File.WriteAllText($"{directory}{fileName}/{fileName}.cs", smartContract);
        }

        public void ValidateSmartContract()
        {

            var smartContractPath = $"{outputPath}/{program.Name}/StratisSmartContract/StratisSmartContract.cs";
            ContractCompilationResult result = CompilationLoader.CompileFromFileOrDirectoryName(smartContractPath);
            if (result is not null && result.Success)
            {
                byte[] hash = HashHelper.Keccak256(result.Compilation);
                uint256 hashDisplay = new uint256(hash);
                Console.WriteLine("Hash");
                Console.WriteLine(hashDisplay.ToString());
                Console.WriteLine("ByteCode");
                Console.WriteLine(result.Compilation.ToHexString());
                Console.WriteLine("\n Congratulations! You have created a smart contract, now lets deploy it!");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine($"Error with validating the smart contract for {program.Name}");
            }
        }
    }
}
