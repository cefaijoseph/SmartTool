namespace SmartTool
{
    using System;

    public interface ISmartToolGenerator
    {
        public void Main();
    }


    public class StratisAttribute : Attribute
    {
    }

    public class IoTDeviceAttribute : Attribute
    {
    }
}
