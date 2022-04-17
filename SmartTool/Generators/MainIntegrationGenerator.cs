using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SmartTool.Generators.Interfaces;

namespace SmartTool.Generators
{
    public class MainIntegrationGenerator : IMainIntegrationGenerator
    {

        public void GenerateMainCode(Type program, string outputPath)
        {
            // Retrieval of fields and methods from the program passed to the tool
            var mainFields = program.GetFieldsWithoutAttribute();
            var mainMethods = program.GetMethodsWithoutAttribute();

            var fieldsCode = string.Join(Environment.NewLine, mainFields.Select(f => $"public {f.FieldType.FullName} {f.Name};").ToArray());

            var methodsCode = string.Join(Environment.NewLine, mainMethods.Select(method =>
            {
                // Decompiles the code of the current method
                string methodCode = program.Decompile(method.MetadataToken);

                // Replacement of main method signature (if exists)
                methodCode = methodCode.Replace("public void Main()", "static async Task Main(string[] args)");

                // Get all functions to call inside the method
                var funtionsToCall = methodCode.GetFunctionCallsFromText();

                // Get all Iot and Smart Contract methods
                var iotMethods = program.GetMethodsByAttribute(nameof(IoTDeviceAttribute));
                var smartContractMethods = program.GetMethodsByAttribute(nameof(SmartContractAttribute));
                var iotSmartContractMethods = iotMethods.Concat(smartContractMethods).ToList();

                foreach (var function in funtionsToCall)
                {
                    var methodName = function.RemoveBetween("(", ")", true);
                    var parametersString = function.GetBetween("(", ")").Trim();
                    var parameters = parametersString.Split(',');
                    var methodInfo = iotSmartContractMethods.Find(x => x.Name == methodName);
                    var parametersWithType = "";
                    if (methodInfo != null && parametersString.Length > 0)
                    {
                        ParameterInfo[] sa;
                        sa = methodInfo.GetParameters();
                        var parametersWithTypeList = parameters.Select((paramName, index) => new ParametersWithType() { Type = sa[index].ParameterType.Name, Name = paramName }).ToList();
                        if (parametersWithTypeList.Count > 0)
                        {
                            parametersWithType = string.Join(", ", parametersWithTypeList.Select(x => $"new ParametersWithType(){{Name = {x.Name}.ToString(),Type = \"{x.Type}\"}}").ToList());
                        }
                    }

                    if (methodInfo != null)
                    {
                        var methodAttribute = methodInfo.CustomAttributes.FirstOrDefault();
                        if (methodAttribute != null)
                        {
                            var methodLocation = methodAttribute.AttributeType.Name == nameof(IoTDeviceAttribute)
                                ? LocationType.IoTDevice :
                                methodAttribute.AttributeType.Name == nameof(SmartContractAttribute)
                                    ? LocationType.Blockchain
                                    : LocationType.Main;
                            var returnType = methodInfo.ReturnType == typeof(void)
                                ? "System.Boolean"
                                : methodInfo.ReturnType.FullName;
                            var runtimeCall = $"await RuntimeCall<{returnType}>(\"{methodLocation}\",\"{methodName}\", runtimeSettings {(parametersWithType.Length > 0 ? $", {parametersWithType}" : "")})";
                            methodCode = methodCode.Replace(function, runtimeCall);
                        }
                    }
                }

                return methodCode;
            }));

            var fileName = "MainApp";
            var directory = $"{outputPath}/{program.Name}/";
            var mainAppCode = @$"
                using System;

                using System.IO;
                using System.Linq;
                using System.Text;
                using System.Reflection;
                using System.Collections.Generic;
                using System.Runtime.CompilerServices;
                using SmartTool;
                using static SmartTool.ToolCalls;


                public class {fileName}
                {{

                    static RuntimeSettings runtimeSettings = new RuntimeSettings()
                    {{
                        ContractAddress = "",
                        Sender = ""
                    }};

                    {fieldsCode}

                    {methodsCode}
                }}";

            // Project references
            var projectReferences = new List<ProjectReference>();
            var smartToolUtilitiesDll = $"{Directory.GetCurrentDirectory()}\\SmartTool.Utilities.dll";
            projectReferences.Add(new ProjectReference() { DllPath = smartToolUtilitiesDll, Name = "SmartTool.Utilities" });
            projectReferences.Add(new ProjectReference() { Name = "Newtonsoft.Json", Version = "13.0.1" });

            // Csproj code generation
            var csprojCode = CsprojGenerator.GenerateCsproj(fileName, projectReferences, OutputType.ClassLibrary);

            // Fix usings
            mainAppCode = mainAppCode.FixUsings();

            if (!Directory.Exists($"{directory}{fileName}"))
            {
                Directory.CreateDirectory($"{directory}{fileName}");
            }

            // Create files
            File.WriteAllText($"{directory}{fileName}/{fileName}.csproj", csprojCode);
            File.WriteAllText($"{directory}{fileName}/{fileName}.cs", mainAppCode);
        }
    }
}
