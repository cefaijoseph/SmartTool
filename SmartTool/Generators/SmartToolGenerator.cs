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
            this.smartToolGeneratorSettings = settings;
            this.LoadInformation();
        }

        public void GenerateIotCode()
        {
            IIoTGenerator ioTGenerator = this.GetIoTGenerator();
            ioTGenerator.GenerateIotCode(this.program, this.smartToolGeneratorSettings.OutputPath);
        }

        public void GenerateMainCode()
        {
            throw new NotImplementedException();
        }

        public void GenerateSmartContract()
        {
            ISmartContractGenerator ioTGenerator = this.GetSmartContractGenerator();
            ioTGenerator.GenerateSmartContract(this.program, this.smartToolGeneratorSettings.OutputPath);
        }

        private void LoadInformation()
        {
            // Gets the type of the compile file, which should inherit ISmartToolTemplate
            var assembly = Assembly.LoadFile(this.smartToolGeneratorSettings.DllPath);
            var smartToolInterface = assembly.GetTypes().Where(x => x.Name == nameof(ISmartToolTemplate)).FirstOrDefault();
            this.program = assembly.GetTypes().Where(t => t != null && t != smartToolInterface
                && !t.GetTypeInfo().IsAbstract && smartToolInterface.IsAssignableFrom(t)).FirstOrDefault();
        }

        private ISmartContractGenerator GetSmartContractGenerator()
        {
            switch (this.smartToolGeneratorSettings.SmartContractType)
            {
                case SmartContractType.Stratis:
                    return new StratisSmartContractGenerator();
                default:
                    return null;
            }
        }

        private IIoTGenerator GetIoTGenerator()
        {
            switch (this.smartToolGeneratorSettings.IoTType)
            {
                case IoTType.RaspberryPi:
                    return new RaspberryPiGenerator();
                default:
                    return null;
            }
        }
    }
}
