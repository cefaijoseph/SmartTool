using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTool
{
    public class CallSmartContractRequest
    {
        public CallSmartContractRequest(
          string methodName,
          List<string> parameters,
          string contractAddress,
          string password,
          string walletName,
          string sender)
        {
            this.MethodName = methodName;
            this.Parameters = parameters;
            this.ContractAddress = contractAddress;
            this.Password = password;
            this.WalletName = walletName;
            this.Sender = sender;
        }

        public ulong Amount { get; set; }

        public double FeeAmount { get; set; } = 0.001;

        public ulong GasPrice { get; set; } = 100;

        public ulong GasLimit { get; set; } = 50000;

        public List<string> Parameters { get; set; } = new List<string>();

        public string MethodName { get; set; }

        public string ContractAddress { get; set; }

        public string Password { get; set; }

        public string WalletName { get; set; }

        public string Sender { get; set; }
    }

    public class CallSmartContractResponse
    {
        public double Fee { get; set; }
        public string Hex { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public string TransactionId { get; set; }
    }

    public class GetReceiptResponse
    {
        public string TransactionHash { get; set; }
        public string BlockHash { get; set; }
        public int BlockNumber { get; set; }
        public string PostState { get; set; }
        public int GasUsed { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string NewContractAddress { get; set; }
        public bool Success { get; set; }
        public string ReturnValue { get; set; }
        public string Bloom { get; set; }
        public string Error { get; set; }
        public List<object> Logs { get; set; }
    }

    public class ParametersWithType
    {
        public string Type { get; set; }

        public string Name { get; set; }
    }

}
