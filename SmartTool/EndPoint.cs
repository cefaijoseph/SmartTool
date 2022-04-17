namespace SmartTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class EndPoint
    {
        public ParameterInfo[] Parameters { get; set; }
        public string FunctionName { get; set; }
        public string Code { get; set; }
    }
}
