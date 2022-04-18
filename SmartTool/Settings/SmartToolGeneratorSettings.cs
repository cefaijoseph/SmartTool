namespace SmartTool.Settings
{
    using System.ComponentModel.DataAnnotations;
    using LanguageTypes;

    public class SmartToolGeneratorSettings
    {
        public SmartToolGeneratorSettings(string dllPath, IoTType ioTType, SmartContractType smartContractType, bool validateSmartContract, string outputPath)
        {
            DllPath = dllPath;
            IoTType = ioTType;
            SmartContractType = smartContractType;
            ValidateSmartContract = validateSmartContract;
            OutputPath = outputPath;
        }

        [Required]
        public IoTType IoTType { get; set; }

        [Required]
        public SmartContractType SmartContractType { get; set; }

        [Required]
        public string DllPath { get; set; }

        [Required]
        public bool ValidateSmartContract { get; set; }

        public string OutputPath { get; set; }
    }
}
