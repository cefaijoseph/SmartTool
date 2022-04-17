using System;
using System.Linq;
using SmartTool.Generators.Interfaces;

namespace SmartTool.Generators
{
    public class MainIntegrationGenerator : IMainIntegrationGenerator
    {
        public MainIntegrationGenerator()
        {
        }

        public bool GenerateMainCode(Type program, string outputPath)
        {
            // Retrieval of fields and methods from the program passed to the tool
            var mainFields = program.GetFieldsWithoutAttribute();
            var mainMethods = program.GetMethodsWithoutAttribute();

            var fieldsCode = string.Join(Environment.NewLine, mainFields.Select(f => $"public {f.FieldType.FullName} {f.Name};").ToArray());

            var methodsCode = string.Join(Environment.NewLine, mainMethods.Select(method =>
            {
                // Decompiles the code of the current method
                string methodCode = program.Decompile(method.MetadataToken);

                // Replacement of main method signature
                methodCode = methodCode.Replace("public void Main()", "static async Task Main(string[] args)");

                // Get all functions to call inside the method
                var funtionsToCall = methodCode.GetFunctionCallsFromText();


                return methodCode;
            }
    }
    }
