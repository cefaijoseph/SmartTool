using SmartTool.Generators.Interfaces;
using SmartTool.LanguageTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartTool.Generators
{
    public class SmartToolGenerator : ISmartToolGenerator
    {
        private SmartToolGeneratorSettings smartToolGeneratorSettings;
        private Type program;

        public SmartToolGenerator(SmartToolGeneratorSettings settings)
        {
            smartToolGeneratorSettings = settings;
        }

        public void LoadInformation()
        {
            // Gets the type of the compile file, which should inherit ISmartToolTemplate
            var assembly = Assembly.LoadFile(smartToolGeneratorSettings.DllPath);
            var smartToolInterface = assembly.GetTypes().Where(x => x.Name == nameof(ISmartToolTemplate)).FirstOrDefault();
            this.program = assembly.GetTypes().Where(t => t != null && t != smartToolInterface
                && !t.GetTypeInfo().IsAbstract && smartToolInterface.IsAssignableFrom(t)).FirstOrDefault();
        }

        public void GenerateIotCode()
        {
            IIoTGenerator ioTGenerator = GetIoTGenerator();
            ioTGenerator.GenerateIotCode();
        }

        public bool GenerateMainCode()
        {
            throw new NotImplementedException();
        }

        public void GenerateSmartContract()
        {
            ISmartContractGenerator ioTGenerator = GetSmartContractGenerator();
            ioTGenerator.GenerateSmartContract();
        }

        private ISmartContractGenerator GetSmartContractGenerator()
        {
            switch(smartToolGeneratorSettings.SmartContractType)
            {
                case SmartContractType.Stratis:
                    return new StratisSmartContractGenerator();
                default:
                    return null;
            }
        }

        private IIoTGenerator GetIoTGenerator()
        {
            switch(smartToolGeneratorSettings.IoTType)
            {
                case IoTType.RaspberryPi:
                    return new RaspberryPiGenerator();
                default:
                    return null;
            }
        }
    }
}
