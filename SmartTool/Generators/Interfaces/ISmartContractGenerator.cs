﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTool.Generators.Interfaces
{
    public interface ISmartContractGenerator
    {
        public void GenerateSmartContract(Type program, string outputPath);

        public void ValidateSmartContract();
    }
}
