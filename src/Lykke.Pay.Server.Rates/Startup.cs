using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Common.Application;
using Lykke.Common.Entities.Pay;
using Lykke.Pay.Service.Rates.Code;
using Lykke.RabbitMqBroker.Subscriber;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Lykke.Pay.Service.Rates
{
    public class Startup
    {
        private Settings _settings;


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

            services.AddMemoryCache();
            services.AddMvc();
 
        }

     
        private void BuildConfiguration(IServiceCollection services)
        {
            var connectionString = Configuration.GetValue<string>("ConnectionString");
#if DEBUG
            _settings = SettingsReader.SettingsReader.ReadGeneralSettings<Settings>(connectionString);
#else
            _settings = SettingsReader.SettingsReader.ReadGeneralSettings<Settings>(new Uri(connectionString));
#endif


            var connectionsString = $"amqp://{_settings.PayServiceRates.RabbitMq.Username}:{_settings.PayServiceRates.RabbitMq.Password}@{_settings.PayServiceRates.RabbitMq.Host}:{_settings.PayServiceRates.RabbitMq.Port}";
            var subscriberSettings = new RabbitMqSubscriberSettings()
            {
                ConnectionString = connectionsString,
                QueueName = _settings.PayServiceRates.RabbitMq.QuoteFeed,
                ExchangeName = _settings.PayServiceRates.RabbitMq.ExchangeName,
                IsDurable = true
            };
            var subscriber = new RabbitMqSubscriber<PairRate>(subscriberSettings);


            services.AddSingleton(_settings.PayServiceRates);
            services.AddSingleton(new HttpClient());
            services.AddSingleton(subscriber);


        }

       
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();


            app.UseMvc();

        }
    }
}
