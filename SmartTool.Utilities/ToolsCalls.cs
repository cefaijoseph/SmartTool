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
        public static async Task<T> RuntimeCall<T>(string location, string methodName, RuntimeSettings runtimeSettings, List<object> parameters = null)
        {
            {
                if (location == "IotDevice")
                {
                    {
                        return (T)Convert.ChangeType("1", typeof(T));
                    }
                }
                else if (location == "Blockchain")
                {
                    {
                        var callSmartContractRequest = new CallSmartContractRequest(methodName, parameters,
                            runtimeSettings.ContractAddress, runtimeSettings.Password, runtimeSettings.WalletName, runtimeSettings.Sender);
                        var httpClient = new HttpClient
                        { BaseAddress = new Uri("http://localhost:38223/api/smartcontracts/") };
                        var responseData =
                            await httpClient.PostAsJsonAsync("build-and-send-call", callSmartContractRequest);
                        var responseContent = await responseData.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<CallSmartContractResponse>(responseContent);
                        // After transaction build and deployment on the blockchain, getTransaction is called in order to retrieve the receipt (it will include the return values)
                        if (response != null && response.Success)
                        {
                            {
                                var triesLeft = 3;
                                var receiptResponse = new GetReceiptResponse();
                                // Try to retrieve the receipt 3 times, if the response is not as expected
                                while (triesLeft != 0 && !receiptResponse.Success)
                                {
                                    {
                                        //Wait for the block to be mined
                                        await Task.Delay(6000);
                                        var receiptData =
                                            await httpClient.GetAsync($"receipt ? txHash ={response.TransactionId}");
                                        var receiptContent = await receiptData.Content.ReadAsStringAsync();
                                        receiptResponse =
                                            JsonConvert.DeserializeObject<GetReceiptResponse>(receiptContent);
                                        if (!response.Success)
                                            triesLeft--;
                                    }
                                }
                                return (T)Convert.ChangeType(receiptResponse?.ReturnValue, typeof(T));
                            }
                        }
                    }
                }

                return (T)Convert.ChangeType(null, typeof(T));
            }
        }
    }

    public class RuntimeSettings
    {
        public string ContractAddress { get; set; }
        public string WalletName { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
    }

    public class ParametersWithType
    {
        public ParametersWithType()
        {
        }

        public string Type { get; set; }
        public string Name { get; set; }
    }
}
