using System.IO;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;

namespace Library.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseNLog()
                .Build();

            host.Run();
        }
    }
}
