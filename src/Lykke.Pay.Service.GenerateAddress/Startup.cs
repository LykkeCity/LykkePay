using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Log;
using Lykke.AzureRepositories;
using Lykke.Pay.Service.GenerateAddress.Code;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lykke.SettingsReader;
using Lykke.Signing.Client;

namespace Lykke.Pay.Service.GenerateAddress
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

        private void BuildConfiguration(IServiceCollection services)
        {
            var connectionString = Configuration.GetValue<string>("ConnectionString");
           
#if DEBUG
            var settings = SettingsReader.SettingsReader.ReadGeneralSettings<Settings>(connectionString);
#else
            var settings = SettingsReader.SettingsReader.ReadGeneralSettings<Settings>(new Uri(connectionString));
#endif


          

            services.AddSingleton(settings.PayServiceGenAddress);
            services.RegisterRepositories(settings.PayServiceGenAddress.Db.PrivateKeysConnString, (ILog)null);
            services.AddSingleton<ILykkeSigningAPI>(new LykkeSigningAPI(new Uri(settings.PayServiceGenAddress.Services.SignServiceUrl)));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
