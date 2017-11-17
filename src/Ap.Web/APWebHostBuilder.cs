using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;

namespace Ap.Web
{
    public class ApWebHostBuilder
    {
       
        public static void BuildAndRun<TStartup>(IConfiguration config,Action<WebHostBuilder> webHostBuilderAction=null) where TStartup : class
        {
            var builder = new WebHostBuilder();
            if(null!=webHostBuilderAction)
            {
                webHostBuilderAction(builder);
            }
            var host = builder.UseConfiguration(config)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(services => services.AddSingleton(config))
                .UseStartup<TStartup>()
                .Build();

            host.Run();
        }
    }
}
