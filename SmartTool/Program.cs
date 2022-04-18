namespace SmartTool
{
    using Generators;
    using Settings;
    using static Utilities.ConsolePrompts;

    class Program
    {
        private static void Main(string[] args)
        {
            // Asks user to enter the path of the DLL
            var dllPath = GetDllPathFromUser();

            // Asks the user to enter the path of the output where the generated code will be saved. If the output path is not specified the code will be saved next to the DLL.
            var outputPath = GetOutputPathFromUser(dllPath);

            // Asks the user if the smart contract should be validated when the tool is executed
            var validateSmartContract = SmartContractValdiationPrompt();

            // Eventually when other platforms are integrated within the tool, the language type of both smart contract and IoT can be specified by the user
            var smartToolGeneratorSettings = new SmartToolGeneratorSettings(dllPath, LanguageTypes.IoTType.RaspberryPi, LanguageTypes.SmartContractType.Stratis, validateSmartContract, outputPath);
            var smartToolGenerator = new SmartToolGenerator(smartToolGeneratorSettings);

            // Generates IoT project
            smartToolGenerator.GenerateIotCode();

            // Generates the main application with integration to both IoT and Smart contract
            smartToolGenerator.GenerateMainCode();

            // Generates the smart contract and validate it if specified
            smartToolGenerator.GenerateSmartContract();
        }
    }
}
