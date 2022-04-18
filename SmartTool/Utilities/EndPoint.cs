namespace SmartTool.Utilities
{
    using System.Reflection;

    public class EndPoint
    {
        public ParameterInfo[] Parameters { get; set; }
        public string FunctionName { get; set; }
        public string Code { get; set; }
    }
}
