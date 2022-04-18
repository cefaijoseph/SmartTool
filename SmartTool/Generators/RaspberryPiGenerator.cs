using SmartTool.Generators.Interfaces;
using SmartTool.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SmartTool.Settings;

namespace SmartTool.Generators
{
    public class RaspberryPiGenerator : IIoTGenerator
    {
        public void GenerateIotCode(Type program, string outputPath)
        {
            // Retrieval of fields and methods from the program passed to the tool
            var iotFields = program.GetFieldsByAttribute(nameof(IoTDeviceAttribute));
            var iotMethods = program.GetMethodsByAttribute(nameof(IoTDeviceAttribute));

            // Initialization of list of endpoints for the Web Api
            var endpoints = new List<EndPoint>();

            foreach(MethodInfo method in iotMethods)
            {

                // Decompiles the code of the current method
                var methodCode = program.Decompile(method.MetadataToken);

                // Removes the attributes from the code
                foreach(var att in method.CustomAttributes)
                {
                    var name = att.AttributeType.Name.Replace("Attribute", "");
                    methodCode = methodCode.Replace($"[{name}]", "");
                }

                // Removal of unnecessary code
                var start = methodCode.IndexOf('{');
                var end = methodCode.LastIndexOf('}');
                methodCode = methodCode.Substring(start + 1, end - start - 1);

                // Adds endpoint to list
                endpoints.Add(new EndPoint() { Code = methodCode, FunctionName = method.Name, Parameters = method.GetParameters() });
            }

            // Api code generation
            var apiSettings = new ApiSettings() { EndPoints = endpoints, Fields = iotFields };
            var apiCode = ApiGenerator.GenerateApi(apiSettings);

            // Project references
            var projectReferences = new List<ProjectReference>
            {
                new ProjectReference() {Name = "Iot.Device.Bindings", Version = "2.1.0"},
                new ProjectReference() {Name = "Microsoft.AspNetCore.Hosting", Version = "2.2.7"},
                new ProjectReference() {Name = "Microsoft.AspNetCore.Server.Kestrel", Version = "2.2.0"},
                new ProjectReference()
                {
                    Name = "Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv", Version = "5.0.15"
                }
            };

            var fileName = "IotApp";
            var directory = $"{outputPath}/{program.Name}/";

            // Csproj code generation
            var csprojCode = CsprojGenerator.GenerateCsproj(fileName, projectReferences);

            // Fix usings
            apiCode = apiCode.FixUsings();

            if(!Directory.Exists($"{directory}{fileName}"))
            {
                Directory.CreateDirectory($"{directory}{fileName}");
            }

            // Create files
            File.WriteAllText($"{directory}{fileName}/{fileName}.csproj", csprojCode);
            File.WriteAllText($"{directory}{fileName}/{fileName}.cs", apiCode);
        }
    }
}
