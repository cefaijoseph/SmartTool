namespace SmartTool.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CsprojGenerator
    {
        public static string GenerateCsproj(string name, List<ProjectReference> references, OutputType outputType = OutputType.ConsoleApplication, string targetFramework = "net5.0")
        {
            var packageReferences = string.Join(Environment.NewLine, references.Where(x => !x.IsDll).Select(p => $"<PackageReference Include=\"{p.Name}\" Version=\"{p.Version}\" />").ToArray());
            var dllReferences = string.Join(Environment.NewLine, references.Where(x => x.IsDll).Select(p => $"<Reference Include=\"{p.Name}\">\r\n      <HintPath>{p.DllPath}</HintPath>\r\n    </Reference>").ToArray());
            return $@"<Project Sdk=""Microsoft.NET.Sdk"">

                <PropertyGroup>
                <OutputType>{(outputType == OutputType.ConsoleApplication ? "Exe" : outputType == OutputType.WindowsAppliaction ? "WinExe" : outputType == OutputType.ClassLibrary ? "Library" : "")}</OutputType>
                <TargetFramework>{targetFramework}</TargetFramework>

                </PropertyGroup>


                <ItemGroup>

                {packageReferences}

                </ItemGroup>


                <ItemGroup>

                {dllReferences}

                </ItemGroup>


                </Project> ";
        }
    }

    public class ProjectReference
    {
        public string Name { get; set; }
        public string Version { get; set; } = "";
        public bool IsDll => DllPath.Length > 0;
        public string DllPath { get; set; } = "";
    }

    public enum OutputType
    {
        ConsoleApplication,
        WindowsAppliaction,
        ClassLibrary
    }
}
