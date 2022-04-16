namespace SmartTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Metadata;
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using ICSharpCode.Decompiler;
    using ICSharpCode.Decompiler.CSharp;
    using ICSharpCode.Decompiler.CSharp.OutputVisitor;
    using ICSharpCode.Decompiler.CSharp.Syntax;
    using Microsoft.Extensions.DependencyModel;
    using NBitcoin;
    using Stratis.SmartContracts.CLR.Compilation;
    using Stratis.SmartContracts.Core.Hashing;
    using Stratis.SmartContracts.Tools.Sct;
    using StratisTool;

    class Program
    {
        static void Main(string[] args)
        {
            // Asks user to enter the path of the DLL
            var dllPath = GetDllPathFromUser();

            // Asks the user to enter the path of the output where the generated code will be saved. If the output path is not specified the code will be saved next to the DLL.
            var outputPath = GetOutputPathFromUser(dllPath);

            // Asks the user if the smart contract should be validated when the tool is executed
            var continueWithValidation = SmartContractValdiationPrompt();



            // Gets the type of the compile file, which should inherit ISmartToolTemplate
            var assembly = Assembly.LoadFile(dllPath);
            var attributes = assembly.GetTypes().Where(x => x.BaseType == typeof(System.Attribute)).ToList();
            var iotAttribute = attributes.Where(x => x.Name == nameof(IoTDeviceAttribute)).FirstOrDefault();
            var stratisAttribute = attributes.Where(x => x.Name == nameof(StratisAttribute)).FirstOrDefault();
            var smartToolInterface = assembly.GetTypes().Where(x => x.Name == nameof(ISmartToolTemplate)).FirstOrDefault();
            var type = assembly.GetTypes().Where(t => t != null && t != smartToolInterface
                && !t.GetTypeInfo().IsAbstract && smartToolInterface.IsAssignableFrom(t)).FirstOrDefault();

            if(!Directory.Exists($"{outputPath}/{type.Name}"))
            {
                Directory.CreateDirectory($"{outputPath}/{type.Name}");
            }

            // fields
            var fields = type.GetFields();

            // properties
            var properties = type.GetProperties();

            // methods
            var methods = type.GetMethods();

            var constructor = type.GetConstructors().ToList();

            // IoT
            var iotFields = fields.Where(f => f.GetCustomAttribute(iotAttribute) != null).ToList();
            var iotProperties = properties.Where(f => f.GetCustomAttribute(iotAttribute) != null).ToList();
            var iotMethods = methods.Where(f => f.GetCustomAttribute(iotAttribute) != null).ToList();
            CreateFile(type, $"{outputPath}/{type.Name}/", "IoTApp", iotFields, iotProperties, iotMethods, constructor, new FileSettings() { LocationType = LocationType.IoTDevice });

            // Stratis
            var stratisFields = fields.Where(f => f.GetCustomAttribute(stratisAttribute) != null).ToList();
            var stratisProperties = properties.Where(f => f.GetCustomAttribute(stratisAttribute) != null).ToList();
            var stratisMethods = methods.Where(f => f.GetCustomAttribute(stratisAttribute) != null).ToList();
            CreateFile(type, $"{outputPath}/{type.Name}/", "StratisApp", stratisFields, stratisProperties, stratisMethods, constructor, new FileSettings() { LocationType = LocationType.Blockchain });

            // Rest is console
            var consoleFields = fields
                .Where(f => f.GetCustomAttribute(iotAttribute) == null)
                .Where(f => f.GetCustomAttribute(stratisAttribute) == null)
                .ToList();
            var consoleProperties = properties
                .Where(f => f.GetCustomAttribute(iotAttribute) == null)
                .Where(f => f.GetCustomAttribute(stratisAttribute) == null)
                .ToList();
            var consoleMethods = methods
                .Where(f => f.GetCustomAttribute(iotAttribute) == null)
                .Where(f => f.GetCustomAttribute(stratisAttribute) == null)
                .Where(f => f.Module.Name == type.Assembly.ManifestModule.Name)
                .ToList();

            CreateFile(type, $"{outputPath}/{type.Name}/", "ConsoleApp", consoleFields, consoleProperties, consoleMethods, constructor, new FileSettings() { LocationType = LocationType.Main, Methods = iotMethods.Concat(stratisMethods).ToList(), IotAttribute = iotAttribute, StratisAttribute = stratisAttribute, DllPath = dllPath });

            // Validate SmartContract
            if(continueWithValidation)
            {
                var smartContractPath = $"{outputPath}\\{type.Name}\\StratisApp\\StratisApp.cs";
                ContractCompilationResult result = CompilationLoader.CompileFromFileOrDirectoryName(smartContractPath);
                if(result is not null && result.Success)
                {
                    byte[] hash = HashHelper.Keccak256(result.Compilation);
                    uint256 hashDisplay = new uint256(hash);
                    Console.WriteLine("Hash");
                    Console.WriteLine(hashDisplay.ToString());
                    Console.WriteLine("ByteCode");
                    Console.WriteLine(result.Compilation.ToHexString());
                    Console.WriteLine("\n Congratulations! You have created a smart contract, now lets deploy it!");
                    Console.ReadLine();
                } else
                {
                    Console.WriteLine($"Error with validating the smart contract for {type.Name}");
                }
            }
        }

        private static string GetDllPathFromUser()
        {
            var dllPath = string.Empty;
            while(dllPath == string.Empty)
            {
                Console.WriteLine("Enter the DLL path of the project that contains the file which inherits ISmartToolGenerator.");
                dllPath = Console.ReadLine();
                if(!File.Exists(dllPath))
                {
                    Console.WriteLine("Incorrect path!");
                    dllPath = string.Empty;
                }
            }
            return dllPath;
        }

        private static string GetOutputPathFromUser(string dllPath)
        {
            var outputPath = string.Empty;
            while(outputPath == string.Empty)
            {
                Console.WriteLine("Enter the preferred output folder location (if not specified files will be located in the bin folder).");
                outputPath = Console.ReadLine();
                if(outputPath == string.Empty)
                {
                    outputPath = Path.GetDirectoryName(dllPath);
                }

                if(!Directory.Exists(outputPath))
                {
                    Console.WriteLine("Incorrect path!");
                    outputPath = string.Empty;
                }
            }

            return outputPath;
        }

        private static bool SmartContractValdiationPrompt()
        {
            var continueWithValidation = false;
            Console.WriteLine("Do you want to validate the smart contract?");
            var key = Console.ReadKey(false).Key;
            if(key == ConsoleKey.Y)
            {
                continueWithValidation = true;
            }
            return continueWithValidation;
        }

        static void WriteCode(TextWriter output, DecompilerSettings settings, SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true });
            TokenWriter tokenWriter = new TextWriterTokenWriter(output) { IndentationString = settings.CSharpFormattingOptions.IndentationString, Indentation = 6 };
            tokenWriter = TokenWriter.WrapInWriterThatSetsLocationsInAST(tokenWriter);
            syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, settings.CSharpFormattingOptions));
        }


        private static void CreateFile(Type type, string dir, string filename, List<FieldInfo> fields, List<PropertyInfo> properties, List<MethodInfo> methods, List<ConstructorInfo> constructors,
            FileSettings settings)
        {
            var isBlockchain = settings.LocationType == LocationType.Blockchain;
            var isIot = settings.LocationType == LocationType.IoTDevice;
            var isMain = settings.LocationType == LocationType.Main;
            var endpoints = new List<EndPoint>();
            var constructorCode = string.Empty;
            var fieldsCode = string.Empty;

            if(isBlockchain)
            {
                fieldsCode = string.Join(Environment.NewLine, fields.Select(f =>
                {
                    return
                        $@"
                        public {f.FieldType.FullName} {f.Name} {{
	                        get => this.PersistentState.Get{(f.FieldType.BaseType?.Name == "Array" ? $"Array<{f.FieldType.FullName.Replace("[]", "")}>" : f.FieldType.Name)}(nameof(this.{f.Name}));
	                        private set => this.PersistentState.Set{(f.FieldType.BaseType?.Name == "Array" ? "Array" : f.FieldType.Name)}(nameof(this.{f.Name}), value);
                        }}";
                })
                        .ToArray());

            }

            if(isMain)
            {
                fieldsCode = string.Join(Environment.NewLine, fields.Select(f => $"public {f.FieldType.FullName} {f.Name};").ToArray());
            }

            var propertiesCode = string.Join(Environment.NewLine, properties.Select(f => $"public {f.PropertyType.FullName} {f.Name} {{get;set;}}").ToArray());
            var methodsCode = string.Join(Environment.NewLine, methods.Select(f =>
            {

                string currentMethodCode = "";
                try
                {
                    currentMethodCode = Decompile(type.Assembly.Location, f.MetadataToken);

                    if(isIot)
                    {
                        foreach(var att in f.CustomAttributes)
                        {
                            var name = att.AttributeType.Name.Replace("Attribute", "");
                            currentMethodCode = currentMethodCode.Replace($"[{name}]", "");
                        }

                        var start = currentMethodCode.IndexOf('{');
                        var end = currentMethodCode.LastIndexOf('}');
                        currentMethodCode = currentMethodCode.Substring(start + 1, end - start - 1);
                        endpoints.Add(new EndPoint() { Code = currentMethodCode, FunctionName = f.Name, Parameters = f.GetParameters() });
                    }
                    if(settings.LocationType == LocationType.Main)
                    {
                        currentMethodCode = currentMethodCode.Replace("public void Main()", "static async Task Main(string[] args)");
                        //Get all function calls

                        var funtionsToCall = Regex.Matches(currentMethodCode, "([a-zA-Z])+([(])+(.*|[(]|[)])+([)])").Select(x => x.Value).ToList();
                        foreach(var method in funtionsToCall)
                        {
                            var methodName = RemoveBetween(method, "(", ")", true);
                            var parametersString = GetBetween(method, "(", ")").Trim();
                            var parameters = parametersString.Split(',');
                            var methodInfo = settings.Methods.Find(x => x.Name == methodName);
                            var parametersWithType = "";
                            if(methodInfo != null && parametersString.Length > 0)
                            {
                                ParameterInfo[] sa;
                                sa = methodInfo.GetParameters();
                                var parametersWithTypeList = parameters.Select((paramName, index) => new ParametersWithType() { Type = sa[index].ParameterType.Name, Name = paramName }).ToList();
                                if(parametersWithTypeList.Count > 0)
                                {
                                    parametersWithType = string.Join(", ", parametersWithTypeList.Select(x => $"new ParametersWithType(){{Name = {x.Name}.ToString(),Type = \"{x.Type}\"}}").ToList());
                                }
                            }

                            if(methodInfo != null)
                            {
                                var methodAttribute = methodInfo.CustomAttributes.FirstOrDefault();
                                if(methodAttribute != null)
                                {
                                    var methodLocation = methodAttribute.AttributeType == settings.IotAttribute
                                        ? LocationType.IoTDevice :
                                        methodAttribute.AttributeType == settings.StratisAttribute
                                            ? LocationType.Blockchain
                                            : LocationType.Main;
                                    var returnType = methodInfo.ReturnType == typeof(void)
                                        ? "System.Boolean"
                                        : methodInfo.ReturnType.FullName;
                                    var runtimeCall = $"await RuntimeCall<{returnType}>(\"{methodLocation}\",\"{methodName}\", runtimeSettings {(parametersWithType.Length > 0 ? $", {parametersWithType}" : "")})";
                                    currentMethodCode = currentMethodCode.Replace(method, runtimeCall);
                                }
                            }

                        }
                    }


                    foreach(var att in f.CustomAttributes)
                    {
                        var name = att.AttributeType.Name.Replace("Attribute", "");
                        currentMethodCode = currentMethodCode.Replace($"[{name}]", "");
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine($"Error with generating {settings.LocationType}");
                };
                return currentMethodCode;
            }).ToArray());

            var text = @$"";
            if(!isIot)
            {


                text = @$"
                using System;

                {(isMain ?
                    @"using System.IO;
                using System.Linq;
                using System.Text;
                using System.Reflection;
                using System.Collections.Generic;
                using System.Runtime.CompilerServices;
                using SmartTool;
                using static SmartTool.ToolCalls;" : "")}
                {(isBlockchain ? "using Stratis.SmartContracts;" : "")}



                    {(isBlockchain ? "// The [Deploy] attribute only needs to be specified when more than one class is declared in the file, but specifying it anyway is fine. " +
                                         "[Deploy]" : "")}
                    public class {filename} {(isBlockchain ? ": SmartContract" : "")}
                    {{

                        {(isMain ?
                               @"static RuntimeSettings runtimeSettings = new RuntimeSettings()
                        {
                            ContractAddress = """",
                            Sender = """"
                        };" : "")}

                        //Constructor
                        public {filename}({(isBlockchain ? "ISmartContractState smartContractState" : "")}){(isBlockchain ? ": base(smartContractState)" : "")}
                        {{
                            {constructorCode}
                        }}

                        {fieldsCode}

                        {propertiesCode}

                        {methodsCode}
                    }}
            ";
            } else
            {
                var apiSettings = new ApiSettings() { EndPoints = endpoints, Fields = fields };
                text = ApiCreator.GenerateApi(apiSettings);
            }
            var references = new List<ProjectReference>();
            if(isMain)
            {
                var smartToolUtilitiesDll = $"{Directory.GetCurrentDirectory()}\\SmartTool.Utilities.dll";
                references.Add(new ProjectReference() { DllPath = smartToolUtilitiesDll, Name = "SmartTool.Utilities" });
                references.Add(new ProjectReference() { Name = "Newtonsoft.Json", Version = "13.0.1" });
            }
            if(isBlockchain)
                references.Add(new ProjectReference() { Name = "Stratis.SmartContracts", Version = "1.2.1" });

            if(isIot)
            {
                references.Add(new ProjectReference() { Name = "Iot.Device.Bindings", Version = "2.1.0" });
                references.Add(new ProjectReference() { Name = "Microsoft.AspNetCore.Hosting", Version = "2.2.7" });
                references.Add(new ProjectReference() { Name = "Microsoft.AspNetCore.Server.Kestrel", Version = "2.2.0" });
                references.Add(new ProjectReference() { Name = "Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv", Version = "5.0.15" });
            }


            var csprojCode = "";
            csprojCode = isBlockchain ? CsprojCreator.GenerateCsproj(filename, references, OutputType.ClassLibrary) : CsprojCreator.GenerateCsproj(filename, references);
            text = FixUsings(text);
            text = text.Replace("using SmartTool;", "");
            if(!Directory.Exists($"{dir}{filename}"))
            {
                Directory.CreateDirectory($"{dir}{filename}");
            }
            File.WriteAllText($"{dir}{filename}/{filename}.csproj", csprojCode);
            File.WriteAllText($"{dir}{filename}/{filename}.cs", text);
        }

        public static string RemoveBetween(string sourceString, string startTag, string endTag, bool includeTags = false)
        {
            Regex regex = new Regex(string.Format("{0}(.*?){1}", Regex.Escape(startTag), Regex.Escape(endTag)));
            return regex.Replace(sourceString, includeTags == false ? startTag + endTag : "");
        }

        public static string GetBetween(string sourceString, string firstString, string lastString)
        {
            var pos1 = sourceString.IndexOf(firstString, StringComparison.Ordinal) + firstString.Length;
            var pos2 = sourceString.IndexOf(lastString, StringComparison.Ordinal);
            var finalString = sourceString.Substring(pos1, pos2 - pos1);
            return finalString;
        }

        private static string FixUsings(string code)
        {
            var usings = Regex.Matches(code, "using.*?;").Select(x => x.Value).ToList();
            foreach(var u in usings)
            {
                code = code.Replace(u, "");
            }

            usings = usings.Distinct().ToList();
            var usingsCode = string.Join(Environment.NewLine, usings);
            code = code.Insert(0, usingsCode);
            code = Regex.Replace(code, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
            return code;
        }

        private static string Decompile(string assemblyLocation, int metadataToken)
        {
            var code = string.Empty;
            var csharpOutput = new StringWriter();
            var decompilerSettings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(assemblyLocation, decompilerSettings);
            var st = decompiler.Decompile((MethodDefinitionHandle)MetadataTokens.EntityHandle(metadataToken));
            WriteCode(csharpOutput, decompilerSettings, st);
            return csharpOutput.ToString();
        }
    }
}
