﻿using System;
using System.Net.Http;
using Bitcoint.Api.Client;
using Common.Log;
using Lykke.AzureRepositories;
using Lykke.AzureRepositories.Azure.Tables;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Pay.Service.GenerateAddress.Client;
using Lykke.Pay.Service.Invoces.Client;
using Lykke.Pay.Service.StoreRequest.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using LykkePay.API.Code;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILog = Lykke.Core.Log.ILog;
using LogEntity = Lykke.AzureRepositories.Log.LogEntity;
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
            var connectionString = Configuration.GetValue<string>("ConnectionString");
#if DEBUG
            var generalSettings = SettingsReader.ReadGeneralSettings<Settings>(new Uri(connectionString));
#else
            //var generalSettings = SettingsReader.ReadGeneralSettings<Settings>(connectionString);
            var generalSettings = SettingsReader.ReadGeneralSettings<Settings>(new Uri(connectionString));
#endif
            var appSettings = new ReloadingManager(generalSettings);
            var log = CreateLogWithSlack(services, appSettings);
            services.AddSingleton(log);
            services.RegisterRepositories(generalSettings.PayApi.Db.BitcoinAppRepository, (ILog)null);
            services.AddSingleton(generalSettings.PayApi);
            services.AddSingleton<ILykkePayServiceStoreRequestMicroService>(new LykkePayServiceStoreRequestMicroService(new Uri(generalSettings.PayApi.Services.StoreRequestService)));
            services.AddSingleton<ILykkePayServiceGenerateAddressMicroService>(new LykkePayServiceGenerateAddressMicroService(new Uri(generalSettings.PayApi.Services.GenerateAddressService)));
            services.AddSingleton<IInvoicesservice>(new Invoicesservice(new Uri(generalSettings.PayApi.Services.InvoicesService)));
            services.AddSingleton<IExchangeOperationsServiceClient>(new ExchangeOperationsServiceClient(generalSettings.PayApi.Services.ExchangeOperationsService));
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
