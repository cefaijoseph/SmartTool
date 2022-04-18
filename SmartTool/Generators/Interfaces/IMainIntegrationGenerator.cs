using System;

namespace SmartTool.Generators.Interfaces
{
    public interface IMainIntegrationGenerator
    {
        public void GenerateMainCode(Type program, string outputPath);
    }
}
