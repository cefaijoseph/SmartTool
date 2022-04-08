using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SmartTool
{
    public static class ToolCalls
    {
        public static async Task<T> RuntimeCall<T>(
          string location,
          string methodName,
          RuntimeSettings runtimeSettings,
          params ParametersWithType[] parameters)
        {
            if(location == "IoTDevice")
            {

                var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{runtimeSettings.IotApiPort}/") };
                var response = await httpClient.GetAsync(methodName);
                return typeof(T) == typeof(bool) ? (T)Convert.ChangeType(response.IsSuccessStatusCode, typeof(T)) : (T)Convert.ChangeType(response.Content, typeof(T));
            }
            if(location == "Blockchain")
            {
                List<string> startiParams = new List<string>();
                foreach(ParametersWithType parameter in parameters)
                {
                    string stratisParamType = GetStratisParamType(parameter.Type);
                    startiParams.Add(stratisParamType + parameter.Name);
                }
                var callSmartContractRequest = new CallSmartContractRequest(methodName, startiParams,
                            runtimeSettings.ContractAddress, runtimeSettings.Password, runtimeSettings.WalletName, runtimeSettings.Sender);
                var httpClient = new HttpClient
                { BaseAddress = new Uri("http://localhost:38223/api/smartcontracts/") };
                var responseData =
                    await httpClient.PostAsJsonAsync("build-and-send-call", callSmartContractRequest);
                var responseContent = await responseData.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<CallSmartContractResponse>(responseContent);
                // After transaction build and deployment on the blockchain, getTransaction is called in order to retrieve the receipt (it will include the return values)
                if(response != null && response.Success)
                {
                    {
                        var triesLeft = 3;
                        var receiptResponse = new GetReceiptResponse();
                        // Try to retrieve the receipt 3 times, if the response is not as expected
                        while(triesLeft != 0 && !receiptResponse.Success)
                        {
                            {
                                //Wait for the block to be mined
                                await Task.Delay(6000);
                                var receiptData =
                                    await httpClient.GetAsync($"receipt ? txHash ={response.TransactionId}");
                                var receiptContent = await receiptData.Content.ReadAsStringAsync();
                                receiptResponse =
                                    JsonConvert.DeserializeObject<GetReceiptResponse>(receiptContent);
                                if(!response.Success)
                                    triesLeft--;
                            }
                        }

                        if(typeof(T) == typeof(void) && receiptResponse.Success)
                        {
                            return (T)Convert.ChangeType((receiptResponse == null ? false : (receiptResponse.Success ? true : false)), typeof(T));
                        }

                        return (T)Convert.ChangeType(receiptResponse?.ReturnValue, typeof(T));
                    }
                }
            }
                return (T)Convert.ChangeType(null, typeof(T));
        }


        public class RuntimeSettings
        {
            public string ContractAddress { get; set; } = "PXfDYtbsuJ4Ek9toQCH6doav51rfZTCNjc";
            public string WalletName { get; set; } = "cirrusdev";
            public string Password { get; set; } = "password";
            public string Sender { get; set; } = "PXDQxB4YQQFG3kzwFyDsPsVMwZ1vFz1RiE";
            public int IotApiPort { get; set; } = 5000;
        }

        private static string GetStratisParamType(string type)
        {
            switch(type)
            {
                case "Boolean":
                    return "1#";
                case "Byte":
                    return "2#";
                case "Byte[]":
                    return "10#";
                case "Char":
                    return "3#";
                case "Int32":
                    return "6#";
                case "Int64":
                    return "8#";
                case "Stratis.SmartContracts.Address ":
                    return "9#";
                case "String":
                    return "4#";
                case "UInt32":
                    return "5#";
                case "UInt64":
                    return "7#";
                default:
                    return "";
            }
        }
    }
}
