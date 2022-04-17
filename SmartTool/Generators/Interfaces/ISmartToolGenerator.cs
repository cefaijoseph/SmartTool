using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTool.Generators.Interfaces
{
    public interface ISmartToolGenerator
    {
        public void GenerateIotCode();
        public void GenerateMainCode();
        public void GenerateSmartContract();
    }
}
