using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace LykkePay.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                        options.Limits.MaxRequestBodySize = int.MaxValue)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://*:4500/")
                .UseIISIntegration()

                .Build();



            host.Run();
        }
    }
}
