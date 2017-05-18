using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace LykkePay.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://*:4500/")
                .UseIISIntegration()

                .Build();



            host.Run();
        }
    }
}
