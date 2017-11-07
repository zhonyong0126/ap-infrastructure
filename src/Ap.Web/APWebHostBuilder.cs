using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using NLog.Web;

namespace Ap.Web
{
    public class ApWebHostBuilder
    {
        public static void BuildAndRun<TStartup>(IConfiguration config) where TStartup : class
        {
            NLogBuilder.ConfigureNLog("nlog.config");
            var builder = new WebHostBuilder();
            var host = builder.UseConfiguration(config)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(services => services.AddSingleton(config))
                .UseStartup<TStartup>()
                .UseNLog()
                .Build();

            host.Run();
        }
    }
}
