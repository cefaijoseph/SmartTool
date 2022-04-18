using SmartTool.Generators.Interfaces;
using SmartTool.LanguageTypes;
using System;
using System.Linq;
using System.Reflection;
using SmartTool.Settings;

namespace SmartTool.Generators
{
    public class SmartToolGenerator : ISmartToolGenerator
    {
        private readonly SmartToolGeneratorSettings _smartToolGeneratorSettings;
        private Type _program;

        public SmartToolGenerator(SmartToolGeneratorSettings settings)
        {
            this._smartToolGeneratorSettings = settings;
            this.LoadInformation();
        }

        public void GenerateIotCode()
        {
            IIoTGenerator ioTGenerator = this.GetIoTGenerator();
            ioTGenerator.GenerateIotCode(this._program, this._smartToolGeneratorSettings.OutputPath);
        }

        public void GenerateMainCode()
        {
            IMainIntegrationGenerator mainIntegrationGenerator = new MainIntegrationGenerator();
            mainIntegrationGenerator.GenerateMainCode(this._program, this._smartToolGeneratorSettings.OutputPath);
        }

        public void GenerateSmartContract()
        {
            ISmartContractGenerator smartContractGenerator = this.GetSmartContractGenerator();
            smartContractGenerator.GenerateSmartContract(this._program, this._smartToolGeneratorSettings.OutputPath);

            if (_smartToolGeneratorSettings.ValidateSmartContract)
            {
                smartContractGenerator.ValidateSmartContract();
            }
        }

        private void LoadInformation()
        {
            // Gets the type of the compile file, which should inherit ISmartToolTemplate
            var assembly = Assembly.LoadFile(this._smartToolGeneratorSettings.DllPath);
            var smartToolInterface = assembly.GetTypes().FirstOrDefault(x => x.Name == nameof(ISmartToolTemplate));
            this._program = assembly.GetTypes()
                .FirstOrDefault(t => smartToolInterface != null && (t != smartToolInterface &&
                                                                    !t.GetTypeInfo().IsAbstract &&
                                                                    smartToolInterface.IsAssignableFrom(t)));
        }

        private ISmartContractGenerator GetSmartContractGenerator()
        {
            switch (this._smartToolGeneratorSettings.SmartContractType)
            {
                case SmartContractType.Stratis:
                    return new StratisSmartContractGenerator();
                default:
                    return null;
            }
        }

        private IIoTGenerator GetIoTGenerator()
        {
            switch (this._smartToolGeneratorSettings.IoTType)
            {
                case IoTType.RaspberryPi:
                    return new RaspberryPiGenerator();
                default:
                    return null;
            }
        }
    }
}
