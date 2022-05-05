namespace SmartTool.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class ConsolePrompts
    {
        public static string GetDllPathFromUser()
        {
            var dllPath = string.Empty;
            while(dllPath == string.Empty)
            {
                Console.WriteLine("Enter the DLL path of the project that contains the file which inherits ISmartToolGenerator.");
                dllPath = Console.ReadLine();
                if(!File.Exists(dllPath))
                {
                    Console.WriteLine("Incorrect path!");
                    dllPath = string.Empty;
                }
            }
            return dllPath;
        }

        public static string GetOutputPathFromUser(string dllPath)
        {
            var outputPath = string.Empty;
            while(outputPath == string.Empty)
            {
                Console.WriteLine("Enter the preferred output folder location (if not specified files will be located in the bin folder).");
                outputPath = Console.ReadLine();
                if(outputPath == string.Empty)
                {
                    outputPath = Path.GetDirectoryName(dllPath);
                }

                if(!Directory.Exists(outputPath))
                {
                    Console.WriteLine("Incorrect path!");
                    outputPath = string.Empty;
                }
            }

            return outputPath;
        }

        public static bool SmartContractValdiationPrompt()
        {
            var validateSmartContract = false;
            Console.WriteLine("Do you want to compile the smart contract?");
            var key = Console.ReadKey(false).Key;
            if(key == ConsoleKey.Y)
            {
                validateSmartContract = true;
            }

            return validateSmartContract;
        }
    }
}
