namespace SmartTool.Generators.Interfaces
{
    using System;

    public interface IMainIntegrationGenerator
    {
        public void GenerateMainCode(Type program, string outputPath);
    }
}
