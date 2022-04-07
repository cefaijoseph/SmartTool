using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTool
{
    public class CallSmartContractRequest
    {

        public CallSmartContractRequest(string methodName, List<object> parameters, string contractAddress, string password, string walletName, string sender)
        {
            MethodName = methodName;
            Parameters = parameters;
            ContractAddress = contractAddress;
            Password = password;
            WalletName = walletName;
            Sender = sender;
        }

        public ulong Amount { get; set; } = 0;
        public double FeeAmount { get; set; } = 0.001;
        public ulong GasPrice { get; set; } = 100;
        public ulong GasLimit { get; set; } = 50000;
        public List<object> Parameters { get; set; } = new();
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
}
