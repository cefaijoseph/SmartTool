namespace SmartTool.Settings
{
    using System.Collections.Generic;
    using System.Reflection;
    using Utilities;

    public class ApiSettings
    {
        public List<FieldInfo> Fields { get; set; }
        public List<EndPoint> EndPoints { get; set; }
    }
}
