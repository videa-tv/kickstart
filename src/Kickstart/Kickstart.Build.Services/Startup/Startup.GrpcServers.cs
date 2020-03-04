using System;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using Grpc.Health.V1;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Kickstart.Build.Services.Config;
using Kickstart.Build.Services.Interceptors;
using HealthServiceImpl = Kickstart.Build.Services.Implementation.HealthServiceImpl;
using LogLevel = Grpc.Core.Logging.LogLevel;
using Company.KickstartBuild.Services.Types;
using Kickstart.Build.Services.GrpcCommon.Logging;
using Kickstart.Build.Services.GrpcCommon;

namespace Kickstart.Build.Services.Startup
{
    public static class ServiceProviderExtensions
    {
        private const int DefaultPort = 50095;

        public static Server AddGrpcServices(this 
            IServiceProvider provider, 
            ILoggerFactory loggerFactory, 
            IConfiguration configuration)
        {

            var servicePort = GetPort(configuration);
            GrpcEnvironment.SetLogger(new LogLevelFilterLogger(new GrpcLogger(loggerFactory), LogLevel.Debug));
            GrpcEnvironment.Logger.Debug($@"Starting Build services on port {servicePort}");

            var logger = loggerFactory.CreateLogger<LoggingInterceptor>();

            var builder = new GrpcServerBuilder()
                .AddInsecurePort(servicePort);

            var services = new List<ServerServiceDefinition>
            {
                KickstartBuildService.BindService(provider.GetRequiredService<KickstartBuildServiceImpl>())
                    .Intercept(new LoggingInterceptor(logger, Microsoft.Extensions.Logging.LogLevel.Information)),

                Health.BindService(provider.GetRequiredService<HealthServiceImpl>())
                    .Intercept(new LoggingInterceptor(logger, Microsoft.Extensions.Logging.LogLevel.Debug)),


            };

            //AddTracing(provider, services, configuration);

            builder.AddServices(services);

            builder.AddService(ServerReflection.BindService(new ReflectionServiceImpl(
                KickstartBuildService.Descriptor,
                Health.Descriptor)));

            return builder.Build();


        }

        private static int GetPort(IConfiguration configuration)
        {
            var port = configuration.GetValue<int>("Port");
            return port > 0 ? port : DefaultPort;

        }
        /*
        private static void AddTracing(
            IServiceProvider provider, 
            IList<ServerServiceDefinition> services, 
            IConfiguration configuration)
        {
            var tracingOptions = new TracingOptions(configuration);
            if (!tracingOptions.Enabled)
            {
                return;
            }

            var serverTracingInterceptor = provider.GetRequiredService<ServerTracingInterceptor>();

            for (var i = 0; i < services.Count; i++)
            {
                services[i] = services[i].Intercept(serverTracingInterceptor);
            }

        }
        */
    }

}
