using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kickstart.Build.Services.Startup;

namespace Kickstart.Build.Services
{
    public class App : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private Server _server;
        private ILogger<App> Logger { get; set; }

        public App(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
             _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            Logger = loggerFactory.CreateLogger<App>();

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting application");
            Logger.LogInformation("Environment: {0}", _hostingEnvironment.EnvironmentName);

            _server = _serviceProvider.AddGrpcServices(_loggerFactory, _configuration);
            _server.Start();

            return Task.CompletedTask;

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Begin stopping gRPC server: server.ShutdownAsync(), wait for requests to complete");
            await _server.ShutdownAsync();
            Logger.LogInformation("End stopping gRPC server");

        }

    }

}
