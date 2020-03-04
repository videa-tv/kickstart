using System;
using System.Collections.Generic;
//using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Pass2.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Kickstart.Services
{
    public class Program
    {
        private const int ShutdownWaitTime = 1;

        public static async Task Main(string [] args)
        {
            using (var serviceProviderFactory = new KickstartServiceProviderFactory())
            {
                IConfigurationRoot configurationRoot = null;
                var hostBuilder = new HostBuilder()
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        configurationRoot = builder.Build();
                        builder.AddJsonFile("appsettings.json", false);
                        //.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",optional: true);
                        //.AddEnvironmentVariables(EnvPrefix)
                        // override it with config-center-service variables
                        //.AddConfigCenter("sample-service", EnvPrefix);
                    })

                    .ConfigureLogging((context, builder) =>
                    {
                        builder.AddSerilog(new LoggerConfiguration()
                            .ReadFrom
                            .Configuration(context.Configuration)
                            .CreateLogger());
                    })
                    .UseServiceProviderFactory(serviceProviderFactory)
                    .ConfigureContainer((HostBuilderContext context, KickstartContainer container) =>
                    {
                        container.Configure(configurationRoot);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddHostedService<App>();

                        services.Configure<HostOptions>(opts =>
                        {
                            opts.ShutdownTimeout = TimeSpan.FromMinutes(ShutdownWaitTime);
                        });
                    });

                await hostBuilder.RunConsoleAsync();

            }
        }
    }
}
