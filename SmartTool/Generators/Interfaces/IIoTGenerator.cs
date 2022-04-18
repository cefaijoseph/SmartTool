namespace SmartTool.Generators.Interfaces
{
    using System;

    public interface IIoTGenerator
    {
        public void GenerateIotCode(Type program, string outputPath);
    }
}
