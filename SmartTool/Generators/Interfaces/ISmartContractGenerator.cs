using System;

namespace SmartTool.Generators.Interfaces
{
    public interface ISmartContractGenerator
    {
        public void GenerateSmartContract(Type program, string outputPath);

        public void ValidateSmartContract();
    }
}
