using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.Pay.Job.ProcessRequests.RequestFactory;
using Lykke.Pay.Service.StoreRequest.Client;
using Newtonsoft.Json;

namespace Lykke.Pay.Job.ProcessRequests
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();


            Console.WriteLine("Hello World!");
        }



        static async Task MainAsync(string[] args)
        {
            var storeClient =
                new LykkePayServiceStoreRequestMicroService(
                    new Uri("https://storerequest-dev.lykkex.net/"));


            var response = await storeClient.ApiStoreGetWithHttpMessagesAsync();
            var json = await response.Response.Content.ReadAsStringAsync();
            var requests = JsonConvert.DeserializeObject<List<MerchantPayRequest>>(json);
            foreach (var request in requests)
            {
                var handler = RequestHandler.Create(request);
                await handler.Handle();
            }
        }
    }
}