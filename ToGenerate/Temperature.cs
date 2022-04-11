namespace StratisTool.ToGenerate
{
    using System;
    using System.Threading.Tasks;
    using Iot.Device.CpuTemperature;
    using SmartTool;

    public class Temperature : ISmartToolGenerator
    {
        [Stratis]
        public int Counter;

        [Stratis]
        public int[] Temperatures;


        public void Main()
        {
            while(true)
            {
                var temp = GetTemperature();
                StoreTemperature(temp);
                Task.Delay(10000);
            }
        }

        [IoTDevice]
        public int GetTemperature()
        {
            Random rnd = new Random();
            return rnd.Next(50); //<--- IoT device
        }

        [Stratis]
        public void StoreTemperature(int tempReading)
        {
            Temperatures[Counter] = tempReading;
            IncreaseCounter();
        }

        [Stratis]
        public void IncreaseCounter()
        {
            Counter += 1;
        }
    }
}