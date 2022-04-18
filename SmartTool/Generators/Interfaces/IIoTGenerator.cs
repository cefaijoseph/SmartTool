using System;

namespace SmartTool.Generators.Interfaces
{
    public interface IIoTGenerator
    {
        public void GenerateIotCode(Type program, string outputPath);
    }
}
