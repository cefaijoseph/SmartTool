namespace SmartTool
{
    using System.Collections.Generic;
    using System.Reflection;

    public class FileSettings
    {
        public LocationType LocationType { get; set; }
        public List<MethodInfo> Methods { get; set; }
    }

    public enum LocationType
    {
        Blockchain,
        IoTDevice,
        Main
    }
}