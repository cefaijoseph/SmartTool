namespace SmartTool
{
    using System;

    public interface ISmartToolTemplate
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
