namespace SmartTool.Generators
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Interfaces;
    using LanguageTypes;
    using Settings;

    public class SmartToolGenerator : ISmartToolGenerator
    {
        private readonly SmartToolGeneratorSettings _smartToolGeneratorSettings;
        private Type _program;

        public SmartToolGenerator(SmartToolGeneratorSettings settings)
        {
            _smartToolGeneratorSettings = settings;
            LoadInformation();
        }

        public void GenerateIotCode()
        {
            IIoTGenerator ioTGenerator = GetIoTGenerator();
            ioTGenerator.GenerateIotCode(_program, _smartToolGeneratorSettings.OutputPath);
        }

        public void GenerateMainCode()
        {
            IMainIntegrationGenerator mainIntegrationGenerator = new MainIntegrationGenerator();
            mainIntegrationGenerator.GenerateMainCode(_program, _smartToolGeneratorSettings.OutputPath);
        }

        public void GenerateSmartContract()
        {
            ISmartContractGenerator smartContractGenerator = GetSmartContractGenerator();
            smartContractGenerator.GenerateSmartContract(_program, _smartToolGeneratorSettings.OutputPath);

            if (_smartToolGeneratorSettings.ValidateSmartContract)
            {
                smartContractGenerator.ValidateSmartContract();
            }
        }

        private void LoadInformation()
        {
            // Gets the type of the compile file, which should inherit ISmartToolTemplate
            var assembly = Assembly.LoadFile(_smartToolGeneratorSettings.DllPath);
            var smartToolInterface = assembly.GetTypes().FirstOrDefault(x => x.Name == nameof(ISmartToolTemplate));
            _program = assembly.GetTypes()
                .FirstOrDefault(t => smartToolInterface != null && (t != smartToolInterface &&
                                                                    !t.GetTypeInfo().IsAbstract &&
                                                                    smartToolInterface.IsAssignableFrom(t)));
        }

        private ISmartContractGenerator GetSmartContractGenerator()
        {
            switch (_smartToolGeneratorSettings.SmartContractType)
            {
                case SmartContractType.Stratis:
                    return new StratisSmartContractGenerator();
                default:
                    return null;
            }
        }

        private IIoTGenerator GetIoTGenerator()
        {
            switch (_smartToolGeneratorSettings.IoTType)
            {
                case IoTType.RaspberryPi:
                    return new RaspberryPiGenerator();
                default:
                    return null;
            }
        }
    }
}
