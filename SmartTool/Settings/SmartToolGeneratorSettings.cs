using SmartTool.LanguageTypes;
using System.ComponentModel.DataAnnotations;

namespace SmartTool.Generators
{

    public class SmartToolGeneratorSettings
    {
        [Required]
        public IoTType IoTType { get; set; }

        [Required]
        public SmartContractType SmartContractType { get; set; }

        [Required]
        public string DllPath { get; set; }

        public string OutputPath { get; set; }

        [Required]
        public string ContinueWithValidation { get; set; }
    }
}
