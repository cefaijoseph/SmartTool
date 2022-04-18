namespace SmartTool.Generators.Interfaces
{
    using System;

    public interface ISmartContractGenerator
    {
        public void GenerateSmartContract(Type program, string outputPath);

        public void ValidateSmartContract();
    }
}
