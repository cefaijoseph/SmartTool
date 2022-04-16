using SmartTool.Generators.Interfaces;
using SmartTool.LanguageTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
