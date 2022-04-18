using System.Collections.Generic;
using System.Reflection;
using SmartTool.Utilities;

namespace SmartTool.Settings
{
    public class ApiSettings
    {
        public List<FieldInfo> Fields { get; set; }
        public List<EndPoint> EndPoints { get; set; }
    }
}
