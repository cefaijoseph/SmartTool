namespace StratisTool.ToGenerate
{
    using System.Threading.Tasks;
    using Iot.Device.CpuTemperature;
    using SmartTool;

    public class Temperature : ISmartToolGenerator
    {
        [Stratis]
        public int Counter;

        [Stratis]
        public int[] Temperatures;

        public Temperature()
        {
        }

        public void Main()
        {
            while(true)
            {
                var temp2 = GetTemperature();
                StoreTemperature(temp2);
                Task.Delay(10000);
                if(IsPersonSensed())
                {
                    IncreaseCounter();
                }
            }
        }

        [IoTDevice]
        public int GetTemperature()
        {
            CpuTemperature temperature = new CpuTemperature();
            if(temperature.IsAvailable)
            {
                return (int)temperature.Temperature.DegreesCelsius;
            }
            return -1; //<--- IoT device
        }


        [IoTDevice]
        public bool IsPersonSensed()
        {
            return false; //<--- IoT device
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