namespace SmartTool
{
    using System;
    using System.IO;
    using SmartTool.Generators;

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

            var smartToolGeneratorSettings = new SmartToolGeneratorSettings(dllPath, LanguageTypes.IoTType.RaspberryPi, LanguageTypes.SmartContractType.Stratis, validateSmartContract, outputPath);
            var smartToolGenerator = new SmartToolGenerator(smartToolGeneratorSettings);

            smartToolGenerator.GenerateIotCode();
            smartToolGenerator.GenerateMainCode();
            smartToolGenerator.GenerateSmartContract();
        }

        private static string GetDllPathFromUser()
        {
            var dllPath = string.Empty;
            while (dllPath == string.Empty)
            {
                Console.WriteLine("Enter the DLL path of the project that contains the file which inherits ISmartToolGenerator.");
                dllPath = Console.ReadLine();
                if (!File.Exists(dllPath))
                {
                    Console.WriteLine("Incorrect path!");
                    dllPath = string.Empty;
                }
            }
            return dllPath;
        }

        private static string GetOutputPathFromUser(string dllPath)
        {
            var outputPath = string.Empty;
            while (outputPath == string.Empty)
            {
                Console.WriteLine("Enter the preferred output folder location (if not specified files will be located in the bin folder).");
                outputPath = Console.ReadLine();
                if (outputPath == string.Empty)
                {
                    outputPath = Path.GetDirectoryName(dllPath);
                }

                if (!Directory.Exists(outputPath))
                {
                    Console.WriteLine("Incorrect path!");
                    outputPath = string.Empty;
                }
            }

            return outputPath;
        }

        private static bool SmartContractValdiationPrompt()
        {
            var validateSmartContract = false;
            Console.WriteLine("Do you want to validate the smart contract?");
            var key = Console.ReadKey(false).Key;
            if (key == ConsoleKey.Y)
            {
                validateSmartContract = true;
            }

            return validateSmartContract;
        }
    }
}
