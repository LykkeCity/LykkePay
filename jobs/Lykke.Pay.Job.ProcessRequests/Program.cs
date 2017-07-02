using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.AzureRepositories;
using Lykke.Pay.Job.ProcessRequests.RequestFactory;
using Lykke.Pay.Service.StoreRequest.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Lykke.Pay.Job.ProcessRequests
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();


        }



        static async Task MainAsync(string[] args)
        {
            

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");


            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();
            var connectionString = configuration.GetValue<string>("ConnectionString");
            var generalSettings = SettingsReader.SettingsReader.ReadGeneralSettings<Settings>(connectionString);

            ILykkePayServiceStoreRequestMicroService storeClient =
                new LykkePayServiceStoreRequestMicroService(
                    new Uri(generalSettings.LykkePayJobProcessRequests.Services.LykkePayServiceStoreRequestMicroService));

            var response = await storeClient.ApiStoreGetWithHttpMessagesAsync();
            var json = await response.Response.Content.ReadAsStringAsync();
            var requests = JsonConvert.DeserializeObject<List<MerchantPayRequest>>(json);

            foreach (var request in requests)
            {
                var handler = RequestHandler.Create(request, generalSettings.LykkePayJobProcessRequests);
                await handler.Handle();
            }
        }
    }
}