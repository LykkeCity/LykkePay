using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bitcoint.Api.Client;
using Lykke.AzureRepositories;
using Lykke.Core.Log;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.SettingsReader;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LykkePay.API
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BuildConfiguration(services);
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.Use(next => context => { context.Request.EnableRewind(); return next(context); });
            app.UseDeveloperExceptionPage();

            app.UseMvc();
        }

        private void BuildConfiguration(IServiceCollection services)
        {
            var connectionString = Configuration.GetValue<string>("ConnectionString");
#if DEBUG
            var generalSettings = SettingsReader.ReadGeneralSettings<Settings>(new Uri(connectionString));
#else
            var generalSettings = SettingsReader.ReadGeneralSettings<Settings>(connectionString);
            //var generalSettings = SettingsReader.ReadGeneralSettings<Settings>(new Uri(connectionString));
#endif
            services.RegisterRepositories(generalSettings.PayApi.Db.BitcoinAppRepository, (ILog)null);
            services.AddSingleton(generalSettings.PayApi);
            services.AddSingleton<ILykkePayServiceStoreRequestMicroService>(new LykkePayServiceStoreRequestMicroService(new Uri(generalSettings.PayApi.Services.StoreRequestService)));
            services.AddSingleton<ILykkePayServiceGenerateAddressMicroService>(new LykkePayServiceGenerateAddressMicroService(new Uri(generalSettings.PayApi.Services.GenerateAddressService)));
            services.AddSingleton<IExchangeOperationsServiceClient>(new ExchangeOperationsServiceClient(generalSettings.PayApi.Services.ExchangeOperationsService));
            services.AddSingleton<IBitcoinApi>(new BitcoinApi(new Uri(generalSettings.PayApi.Services.BitcoinApi)));
            services.AddSingleton(new HttpClient());
        }
    }
}
