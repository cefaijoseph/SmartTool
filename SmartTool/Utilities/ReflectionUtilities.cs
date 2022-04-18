namespace SmartTool.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Metadata;
    using System.Reflection.Metadata.Ecma335;
    using System.Text.RegularExpressions;
    using ICSharpCode.Decompiler;
    using ICSharpCode.Decompiler.CSharp;
    using ICSharpCode.Decompiler.CSharp.OutputVisitor;
    using ICSharpCode.Decompiler.CSharp.Syntax;

    public static class ReflectionUtilities
    {
        public static List<FieldInfo> GetFieldsByAttribute(this Type type, string attribute)
        {
            return type.GetFields().Where(x => x.GetCustomAttributes().Any(y => y.GetType().Name == attribute)).ToList();
        }

        public static List<MethodInfo> GetMethodsByAttribute(this Type type, string attribute)
        {
            return type.GetMethods().Where(x => x.GetCustomAttributes().Any(y => y.GetType().Name == attribute)).ToList();
        }

        public static List<FieldInfo> GetFieldsWithoutAttribute(this Type type)
        {
            return type.GetFields().Where(x => x.GetCustomAttributes().Any() == false).ToList();
        }

        public static List<MethodInfo> GetMethodsWithoutAttribute(this Type type)
        {
            return type.GetMethods().Where(x => x.GetCustomAttributes().Any() == false && x.Module.Name == type.Assembly.ManifestModule.Name).ToList();
        }

        public static string Decompile(this Type type, int metadataToken)
        {
            var csharpOutput = new StringWriter();
            var decompilerSettings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(type.Assembly.Location, decompilerSettings);
            var st = decompiler.Decompile((MethodDefinitionHandle)MetadataTokens.EntityHandle(metadataToken));
            WriteCode(csharpOutput, decompilerSettings, st);
            return csharpOutput.ToString();
        }

        private static void WriteCode(TextWriter output, DecompilerSettings settings, SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true });
            TokenWriter tokenWriter = new TextWriterTokenWriter(output) { IndentationString = settings.CSharpFormattingOptions.IndentationString, Indentation = 6 };
            tokenWriter = TokenWriter.WrapInWriterThatSetsLocationsInAST(tokenWriter);
            syntaxTree.AcceptVisitor(new CSharpOutputVisitor(tokenWriter, settings.CSharpFormattingOptions));
        }

        public static string FixUsings(this string code)
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

        public static List<string> GetFunctionCallsFromText(this string text)
        {
            return Regex.Matches(text, "([a-zA-Z])+([(])+(.*|[(]|[)])+([)])").Select(x => x.Value).ToList();
        }
    }
}
