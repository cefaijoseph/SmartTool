using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTool.Generators.Interfaces
{
    public interface IIoTGenerator
    {
        public void GenerateIotCode(Type program, string outputPath);
    }
}
