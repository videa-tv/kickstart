using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Kickstart.Build.Services.Infrastructure;
using Kickstart.Build.Services.Startup;

namespace Kickstart.Build.Services
{
    public class Program
    {
        /// <summary>
        ///number of minutes to wait before forced shutdown
        /// </summary>
        private const int ShutdownWaitTime = 1;
        private const string EnvPrefix = "KICKSTART_BUILD_SVC_";

        public static async Task Main(string[] args)
        {
            using (var serviceProviderFactory = new BuildServiceProviderFactory())
            {
                var hostBuilder = new HostBuilder()
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        builder.AddJsonFile("appsettings.json", optional: false)
                            .AddEnvironmentVariables(prefix: EnvPrefix);
                            // override it with config-center-service variables
                            //.AddConfigCenter("kickstart-service", EnvPrefix);
                    })
                    .ConfigureLogging((context, builder) =>
                    {
                        builder.AddSerilog(new LoggerConfiguration()
                            .ReadFrom
                            .Configuration(context.Configuration)
                            .CreateLogger());
                    })
                    .UseServiceProviderFactory(serviceProviderFactory)
                    .ConfigureContainer((HostBuilderContext context, BuildContainer container) =>
                    {
                        container.Configure(context.Configuration);
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
