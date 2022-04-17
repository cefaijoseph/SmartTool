namespace SmartTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ApiSettings
    {
        public List<FieldInfo> Fields { get; set; }
        public List<EndPoint> EndPoints { get; set; }
    }
}
