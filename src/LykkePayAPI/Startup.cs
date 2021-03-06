﻿using System;
using System.Net.Http;
using Bitcoint.Api.Client;
using Common.Log;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.Invoces.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using Lykke.Pay.Service.Wallets.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using LykkePay.API.Code;
using LykkePay.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogToConsole = Common.Log.LogToConsole;

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
            services.Configure<FormOptions>(
                options =>
                {
                    options.MultipartBodyLengthLimit = int.MaxValue;
                    options.ValueLengthLimit = int.MaxValue;
                    options.MultipartHeadersLengthLimit = int.MaxValue;
                });
            // Add framework services.
            services.AddMvc();

            
            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", "Lykke Pay API");
                options.CustomSchemaIds(x => x.FullName);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMiddleware<RequestLogMiddleware>();

            app.Use(next => context => { context.Request.EnableRewind(); return next(context); });
            app.UseDeveloperExceptionPage();

            app.UseMvc();

            app.UseSwagger(c =>

            {

                c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);

            });

            app.UseSwaggerUI(x =>

            {

                x.RoutePrefix = "swagger/ui";

                x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");

            });

            app.UseStaticFiles();
        }

        private void BuildConfiguration(IServiceCollection services)
        {
            var appSettings = Configuration.LoadSettings<Settings>();
            var generalSettings = appSettings.CurrentValue;
            var log = CreateLogWithSlack(services, appSettings);
            services.AddSingleton(log);
            services.AddSingleton(generalSettings.PayApi);
            services.AddSingleton<ILykkePayServiceStoreRequestMicroService>(new LykkePayServiceStoreRequestMicroService(new Uri(generalSettings.PayApi.Services.StoreRequestService)));
            services.AddSingleton<ILykkePayServiceGenerateAddressMicroService>(new LykkePayServiceGenerateAddressMicroService(new Uri(generalSettings.PayApi.Services.GenerateAddressService)));
            services.AddSingleton<IInvoicesservice>(new Invoicesservice(new Uri(generalSettings.PayApi.Services.InvoicesService)));
            services.AddSingleton<IExchangeOperationsServiceClient>(new ExchangeOperationsServiceClient(generalSettings.PayApi.Services.ExchangeOperationsService));
            services.AddSingleton<IPayWalletservice>(new PayWalletservice(new Uri(generalSettings.PayApi.Services.PayWalletServiceUrl)));
            services.AddSingleton<IBitcoinApi>(new BitcoinApi(new Uri(generalSettings.PayApi.Services.BitcoinApi)));
            services.AddSingleton(new HttpClient());
            services.AddSingleton<IHealthService>(new HealthService(TimeSpan.FromSeconds(30)));
        }

        private static Common.Log.ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<Settings> appSettings)

        {

            var consoleLogger = new LogToConsole();

            var aggregateLogger = new AggregateLogger();



            aggregateLogger.AddLog(consoleLogger);



            // Creating slack notification service, which logs own azure queue processing messages to aggregate log

            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new Lykke.AzureQueueIntegration.AzureQueueSettings

            {

                ConnectionString = appSettings.CurrentValue.PayApi.Db.LogsConnString,

                QueueName = appSettings.CurrentValue.SlackNotifications.AzureQueue.QueueName

            }, aggregateLogger);



            var dbLogConnectionStringManager = appSettings.Nested(x => x.PayApi.Db.LogsConnString);

            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;



            // Creating azure storage logger, which logs own messages to concole log

            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))

            {

                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager("LykkePayApi",

                    AzureStorage.Tables.AzureTableStorage<Lykke.Logs.LogEntity>.Create(dbLogConnectionStringManager, "LykkePayApiLog", consoleLogger),

                    consoleLogger);



                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);



                var azureStorageLogger = new LykkeLogToAzureStorage(

                    persistenceManager,

                    slackNotificationsManager,

                    consoleLogger);



                azureStorageLogger.Start();



                aggregateLogger.AddLog(azureStorageLogger);

            }



            return aggregateLogger;

        }
    }
}
