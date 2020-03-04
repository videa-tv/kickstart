using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Kickstart.Services.Startup;
using Microsoft.Extensions.Hosting;

namespace Kickstart.Services
{
    public class App : IHostedService
    {
        public string EnvironmentName;
        private IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;

        private Server _server;
        public static IConfiguration Configuration { get; set; } 
        private ILogger<App> Logger { get; set; }

        public App(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
            _serviceProvider = serviceProvider;
            Logger = loggerFactory.CreateLogger<App>();

        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting application2");
           // Logger.LogInformation("Environment: {0}", _hostingEnvironment.EnvironmentName);

            _server = _serviceProvider.AddGrpcServices(_loggerFactory, Configuration.GetValue<int>("Port"));
            _server.Start();
        }
        public async  Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Begin stopping gRPC server: server.ShutdownAsync(), wait for requests to complete");
            await _server.ShutdownAsync();
            Logger.LogInformation("End stopping gRPC server");
        }

    }
}
