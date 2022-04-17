namespace SmartTool
{
    using System;

    public interface ISmartToolTemplate
    {
        public void Main();
    }


    public class SmartContractAttribute : Attribute
    {
    }

    public class IoTDeviceAttribute : Attribute
    {
    }
}
