using System.Net.Http;
using Lykke.AzureRepositories;
using Lykke.Pay.Service.Rates.Code;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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


          
            services.AddSingleton(_settings.PayServiceRates);
            services.AddSingleton(new HttpClient());
            services.RegisterRepositories(_settings.PayServiceRates.Db.AssertHistoryConnString, null);
            

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
