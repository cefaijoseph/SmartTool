namespace cefai
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
    using SmartTool;
    using Stratis.SmartContracts.CLR.Compilation;
    using Stratis.SmartContracts.Core.Hashing;
    using Stratis.SmartContracts.Tools.Sct;

    class Program
    {
        static void Main(string[] args)
        {
            //var path = args.FirstOrDefault(x => x.StartsWith("--path=")).Substring(7);
            //if (path is null)
            //{
            //    throw new Exception("You need to pass the path of the program by using '--path=YOURPATH'");
            //}
            // ByteCode of the file written by the user
            //new Compiler().Compile(path);
            //var assembly = Assembly.LoadFile($"{ Directory.GetParent(Assembly.GetEntryAssembly().Location)}\\UserProgramGenerated.dll");
            //var version = assembly.GetName().Version;
            //var assemblyTypes = assembly.GetTypes().Where(t => t != typeof(ICefaiGenerator) && !t.GetTypeInfo().IsAbstract && typeof(ICefaiGenerator).IsAssignableFrom(t))
            //        .Distinct().ToList();
            //if (assemblyTypes.Count == 0)
            //{
            //    throw new Exception("You need to pass the path of the program that inherits ICefaiGenerator!");
            //};

            //var assemblyTypes = FindDerivedTypes("UserProgramGenerated", typeof(ICefaiGenerator));
            var assemblyTypes = FindDerivedTypes("cefai", typeof(ICefaiGenerator));

            foreach (var type in assemblyTypes)
            {
                if (!Directory.Exists($"../{type.Name}"))
                {
                    Directory.CreateDirectory($"../{type.Name}");
                }

                // fields
                var fields = type.GetFields();
                // properties
                var properties = type.GetProperties();
                // methods
                var methods = type.GetMethods();


                // iot
                var iotFields = fields.Where(f => f.GetCustomAttribute<IoTDeviceAttribute>() != null).ToList();
                var iotProperties = properties.Where(f => f.GetCustomAttribute<IoTDeviceAttribute>() != null).ToList();
                var iotMethods = methods.Where(f => f.GetCustomAttribute<IoTDeviceAttribute>() != null).ToList();
                CreateFile(type, $"../{type.Name}/", "IoTApp", iotFields, iotProperties, iotMethods, new FileSettings() { LocationType = LocationType.IoTDevice });

                // stratis
                var stratisFields = fields.Where(f => f.GetCustomAttribute<StratisAttribute>() != null).ToList();
                var stratisProperties = properties.Where(f => f.GetCustomAttribute<StratisAttribute>() != null).ToList();
                var stratisMethods = methods.Where(f => f.GetCustomAttribute<StratisAttribute>() != null).ToList();
                CreateFile(type, $"../{type.Name}/", "StratisApp", stratisFields, stratisProperties, stratisMethods, new FileSettings() { LocationType = LocationType.Blockchain });

                // rest is console
                var consoleFields = fields
                    .Where(f => f.GetCustomAttribute<IoTDeviceAttribute>() == null)
                    .Where(f => f.GetCustomAttribute<StratisAttribute>() == null)
                    .ToList();
                var consoleProperties = properties
                    .Where(f => f.GetCustomAttribute<IoTDeviceAttribute>() == null)
                    .Where(f => f.GetCustomAttribute<StratisAttribute>() == null)
                    .ToList();
                var consoleMethods = methods
                    .Where(f => f.GetCustomAttribute<IoTDeviceAttribute>() == null)
                    .Where(f => f.GetCustomAttribute<StratisAttribute>() == null)
                    .ToList();

                CreateFile(type, $"../{type.Name}/", "ConsoleApp", consoleFields, consoleProperties, consoleMethods, new FileSettings() { LocationType = LocationType.Main, Methods = iotMethods.Concat(stratisMethods).ToList() });
            }

            // Validate SmartContract
            var continueWithValidation = args.Any(x => x == "validate");
            if (continueWithValidation)
            {
                var smartContractPath = $"../{assemblyTypes[0].Name}/StratisApp/StratisApp.cs";
                ContractCompilationResult result = CompilationLoader.CompileFromFileOrDirectoryName(smartContractPath);
                byte[] hash = HashHelper.Keccak256(result.Compilation);
                uint256 hashDisplay = new uint256(hash);
                Console.WriteLine("Hash");
                Console.WriteLine(hashDisplay.ToString());
                Console.WriteLine("ByteCode");
                Console.WriteLine(result.Compilation.ToHexString());
            }
        }


        static void WriteCode(TextWriter output, DecompilerSettings settings, SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true });
            TokenWriter tokenWriter = new TextWriterTokenWriter(output) { IndentationString = settings.CSharpFormattingOptions.IndentationString, Indentation = 6 };
            tokenWriter = TokenWriter.WrapInWriterThatSetsLocationsInAST(tokenWriter);
            syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, settings.CSharpFormattingOptions));
        }


        private static void CreateFile(Type type, string dir, string filename, List<FieldInfo> fields, List<PropertyInfo> properties, List<MethodInfo> methods,
            FileSettings settings)
        {
            var isBlockchain = settings.LocationType == LocationType.Blockchain;
            var isIot = settings.LocationType == LocationType.IoTDevice;
            var isMain = settings.LocationType == LocationType.Main;
            var endpoints = new List<EndPoint>();
            var constructorCode = "";
            var fieldsCode = "";
            if (isBlockchain)
            {
                fieldsCode = string.Join(Environment.NewLine, fields.Select(f =>
                {
                    var a = f;
                    return
                        $@"
                        public {f.FieldType.FullName} {f.Name} {{
	                        get => this.PersistentState.Get{(f.FieldType.BaseType?.Name == "Array" ? $"Array<{f.FieldType.FullName.Replace("[]", "")}>" : f.FieldType.Name)}(nameof(this.{f.Name}));
	                        private set => this.PersistentState.Set{(f.FieldType.BaseType?.Name == "Array" ? "Array" : f.FieldType.Name)}(nameof(this.{f.Name}), value);
                        }}";
                })
                        .ToArray());

            }
            else if (isMain)
            {
                fieldsCode = string.Join(Environment.NewLine, fields.Select(f => $"public {f.FieldType.FullName} {f.Name};").ToArray());

            }

            var propertiesCode = string.Join(Environment.NewLine, properties.Select(f => $"public {f.PropertyType.FullName} {f.Name} {{get;set;}}").ToArray());
            var methodsCode = string.Join(Environment.NewLine, methods.Select(f =>
            {

                string codeLines = "";
                try
                {
                    //var parameters = string.Join(", ", f.GetParameters().Select(f => $"{f.ParameterType.FullName} {f.Name};").ToArray());

                    var csharpOutput = new StringWriter();
                    var decompilerSettings = new DecompilerSettings()
                    {
                    };

                    CSharpDecompiler decompiler = new CSharpDecompiler(type.Assembly.Location, decompilerSettings);
                    var st = decompiler.Decompile((MethodDefinitionHandle)MetadataTokens.EntityHandle(f.MetadataToken));
                    WriteCode(csharpOutput, decompilerSettings, st);
                    codeLines = csharpOutput.ToString();

                    if (isIot)
                    {
                        foreach (var att in f.CustomAttributes)
                        {
                            var name = att.AttributeType.Name.Replace("Attribute", "");
                            codeLines = codeLines.Replace($"[{name}]", "");
                        }

                        var start = codeLines.IndexOf('{');
                        var end = codeLines.LastIndexOf('}');
                        codeLines = codeLines.Substring(start + 1, end - start - 1);
                        endpoints.Add(new EndPoint() { Code = codeLines, FunctionName = f.Name, Parameters = f.GetParameters() });
                    }
                    if (settings.LocationType == LocationType.Main)
                    {
                        codeLines = codeLines.Replace("public void Main()", "static async Task Main(string[] args)");
                        //Get all function calls

                        var funtionsToCall = Regex.Matches(codeLines, "([a-zA-Z])+([(])+(.*|[(]|[)])+([)])").Select(x => x.Value).ToList();
                        foreach (var method in funtionsToCall)
                        {
                            var methodName = RemoveBetween(method, "(", ")", true);
                            var parametersString = GetBetween(method, "(", ")").Trim();
                            var parameters = parametersString.Split(',');
                            var methodInfo = settings.Methods.Find(x => x.Name == methodName);
                            var parametersWithType = "";
                            if (methodInfo != null && parametersString.Length > 0)
                            {
                                ParameterInfo[] sa;
                                sa = methodInfo.GetParameters();
                                var parametersWithTypeList = parameters.Select((paramName, index) => new ParametersWithType() { Type = sa[index].ParameterType.Name, Name = paramName }).ToList();
                                if (parametersWithTypeList.Count > 0)
                                {
                                    parametersWithType = String.Join(", ", parametersWithTypeList.Select(x => $"new ParametersWithType(){{Name = {x.Name}.ToString(),Type = \"{x.Type}\"}}").ToList());
                                }
                            }

                            if (methodInfo != null)
                            {
                                var methodAttribute = methodInfo.CustomAttributes.FirstOrDefault();
                                if (methodAttribute != null)
                                {
                                    var methodLocation = methodAttribute.AttributeType == typeof(IoTDeviceAttribute)
                                        ? LocationType.IoTDevice :
                                        methodAttribute.AttributeType == typeof(StratisAttribute)
                                            ? LocationType.Blockchain
                                            : LocationType.Main;
                                    var returnType = methodInfo.ReturnType == typeof(void)
                                        ? "System.Boolean"
                                        : methodInfo.ReturnType.FullName;
                                    var runtimeCall = $"await RuntimeCall<{returnType}>(\"{methodLocation}\",\"{methodName}\", runtimeSettings {(parametersWithType.Length > 0 ? $", {parametersWithType}" : "")})";
                                    codeLines = codeLines.Replace(method, runtimeCall);
                                }
                            }

                        }
                        //var a = decompiler.Decompile((MethodDefinitionHandle)MetadataTokens.EntityHandle())
                    }


                    foreach (var att in f.CustomAttributes)
                    {
                        var name = att.AttributeType.Name.Replace("Attribute", "");
                        codeLines = codeLines.Replace($"[{name}]", "");
                    }
                }
                catch (Exception ex) { Console.WriteLine($"Error with generating {settings.LocationType}"); };

                return codeLines;
            }).ToArray());
            var text = @$"";
            if (!isIot)
            {


                text = @$"
                using System;

                {(isMain ?
                    @"using System.IO;
                using System.Linq;
                using System.Text;
                using System.Reflection;
                using System.Collections.Generic;
                using System.Runtime.CompilerServices;using CefaiTool;
                using static CefaiTool.ToolCalls;" : "")}
                {(isBlockchain ? "using Stratis.SmartContracts;" : "")}



                    {(isBlockchain ? "// The [Deploy] attribute only needs to be specified when more than one class is declared in the file, but specifying it anyway is fine. " +
                                         "[Deploy]" : "")}
                    public class {filename} {(isBlockchain ? ": SmartContract" : "")}
                    {{

                        {(isMain ?
                               @"static RuntimeSettings runtimeSettings = new RuntimeSettings()
                        {
                            ContractAddress = """",
                            Password = """",
                            WalletName = """",
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
            }
            else
            {
                var apiSettings = new ApiSettings() { EndPoints = endpoints, Fields = fields };
                text = ApiCreator.GenerateApi(apiSettings);
            }
            var references = new List<ProjectReference>();
            if (isMain)
            {
                references.Add(new ProjectReference() { DllPath = "CefaiTool.dll", Name = "CefaiTool" });
                references.Add(new ProjectReference() { Name = "Newtonsoft.Json", Version = "13.0.1" });
            }
            if (isBlockchain)
                references.Add(new ProjectReference() { Name = "Stratis.SmartContracts", Version = "1.2.1" });
            if (isIot)
            {
                references.Add(new ProjectReference() { Name = "Iot.Device.Bindings", Version = "2.1.0" });
                references.Add(new ProjectReference() { Name = "Microsoft.AspNetCore.Hosting", Version = "2.2.7" });
                references.Add(new ProjectReference() { Name = "Microsoft.AspNetCore.Server.Kestrel", Version = "2.2.0" });
                references.Add(new ProjectReference() { Name = "Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv", Version = "5.0.15" });
            }


            var csprojCode = "";
            csprojCode = isBlockchain ? CsprojCreator.GenerateCsproj(filename, references, OutputType.ClassLibrary) : CsprojCreator.GenerateCsproj(filename, references);
            text = FixUsings(text);
            if (!Directory.Exists($"{dir}{filename}")) Directory.CreateDirectory($"{dir}{filename}");
            File.WriteAllText($"{dir}{filename}/{filename}.csproj", csprojCode);
            File.WriteAllText($"{dir}{filename}/{filename}.cs", text);
        }

        private static List<Type> FindDerivedTypes(string dllPrefix, Type baseType)
        {
            return Assemblies(dllPrefix).SelectMany(x => x.GetTypes())
                .Select(t => t.DeclaringType == null
                    ? t
                    : t.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                        ? t.DeclaringType
                        : null)
                .Where(t => t != null && t != baseType && !t.GetTypeInfo().IsAbstract && baseType.IsAssignableFrom(t))
                .Distinct().ToList();
        }

        private static List<Assembly> Assemblies(string dllPrefix)
        {
            var assemblies = new List<Assembly>();

            //Potential location of all assemblies
            var directory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

            //All dlls in directory of the entry assembly
            var dlls = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories).ToList().Distinct()
                .ToList();

            var modelsDlls = dlls.Where(x =>
            {
                var fileName = Path.GetFileName(x);
                return fileName.StartsWith($"{dllPrefix}.");
            }).ToList();

            foreach (var item in modelsDlls)
            {
                var assembly = Load(item);
                if (assembly != null)
                    assemblies.Add(assembly);
            }
            return assemblies;
        }


        public static Assembly Load(string assemblyFullPath)
        {
            try
            {
                var fileNameWithOutExtension = Path.GetFileNameWithoutExtension(assemblyFullPath);

                var inCompileLibraries = DependencyContext.Default?.CompileLibraries?.Any(l =>
                    l.Name.Equals(fileNameWithOutExtension, StringComparison.OrdinalIgnoreCase));
                var inRuntimeLibraries = DependencyContext.Default?.RuntimeLibraries?.Any(l =>
                    l.Name.Equals(fileNameWithOutExtension, StringComparison.OrdinalIgnoreCase));

                var assembly = (inCompileLibraries.GetValueOrDefault() || inRuntimeLibraries.GetValueOrDefault())
                    ? Assembly.Load(new AssemblyName(fileNameWithOutExtension))
                    : Assembly.LoadFile(assemblyFullPath);

                return assembly;
            }
            catch
            {
                return null;
            }
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
            foreach (var u in usings)
            {
                code = code.Replace(u, "");
            }

            usings = usings.Distinct().ToList();
            var usingsCode = string.Join(Environment.NewLine, usings);
            code = code.Insert(0, usingsCode);
            code = Regex.Replace(code, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
            return code;
        }
    }
}
