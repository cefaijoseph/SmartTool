namespace StratisTool.ToGenerate
{
    using System;
    using System.Threading.Tasks;
    using SmartTool;

    public class Temperature : ISmartToolTemplate
    {
        // Note: Stratis and IotDevice attributes indicate where the variables and methods will live
        // Methods can only access variables which live on the same platform, meaning a method with a Stratis attribute can only only access a variable with a Stratis attribute

        [SmartContract]
        public int TemperatureReadings;

        [SmartContract]
        public int CurrentTemperature;

        // The main function will contain the logic need between in order to integrate both Blockchain and IoT devices together.
        public void Main()
        {
            while (true)
            {
                //Retrieves the data from the Api, which will eventually live next to an IoT device
                var temp = GetTemperature();

                // Stores the data on the blockchain
                StoreTemperature(temp);

                // Waits 10 seconds before process is iterated
                Task.Delay(10000);
            }
        }

        [IoTDevice]
        public int GetTemperature()
        {
            // A random number is returned, in order to substitute the IoT data.
            Random rnd = new Random();
            return rnd.Next(50);
        }

        [SmartContract]
        public void StoreTemperature(int tempReading)
        {
            // Update the temperature reading on the blockchain
            CurrentTemperature = tempReading;

            //Increase temperature reading
            TemperatureReadings += 1;
        }
    }
}