using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace cefai
{
    public class ApiCreator
    {
        public static string GenerateApi(ApiSettings apiSettings)
        {
            //if (apiSettings.EndPoints.Count == 0)
            //    throw new ArgumentException("You need to have at least one endpoint");
            var usings = $@"using System;
using System.Collections.Generic;
using System.Threading;
using Iot.Device.CpuTemperature;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;";
            var fields = string.Join(Environment.NewLine, apiSettings.Fields.Select(x => $"private static {x.FieldType.FullName} {x.Name};"));
            var routesCode = string.Join(Environment.NewLine, apiSettings.EndPoints
                .Select(x =>
                    $@"[Route(""{x.FunctionName}"")]
                    public int {x.FunctionName}({string.Concat(x.Parameters.Select((y, index) => $"{(index != 0 ? ", " : "")}{y.ParameterType.FullName} {y.Name}").ToArray())})
                    {{
                    {x.Code}
                    }}"
                ).ToArray());
            return $@"{usings}
public class HomeController : Controller
    {{
        {fields}

        {routesCode}
        // --------
    }}

    class Program
    {{
        static void Main(string[] args)
        {{

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();


            host.Run();
        }}
    }}

    public class Startup
    {{
        public void ConfigureServices(IServiceCollection services)
        {{
            services.AddControllers();
        }}

        public void Configure(IApplicationBuilder app)
        {{
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {{
                endpoints.MapControllers();
            }});

        }}
    }}";
        }
    }

    public class ApiSettings
    {
        public List<FieldInfo> Fields { get; set; }
        public List<EndPoint> EndPoints { get; set; }
    }

    public class EndPoint
    {
        public ParameterInfo[] Parameters { get; set; }
        public string FunctionName { get; set; }
        public string Code { get; set; }
    }
}
