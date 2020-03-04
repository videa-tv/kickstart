using System;
using System.Collections.Generic;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Grpc.Core.Logging;
using Grpc.Reflection.V1Alpha;
using Grpc.Reflection;
using Grpc.Health.V1;
using Kickstart.Build.Services;
using Kickstart.Services.Types;
using Company.KickstartBuild.Services.Types;
using Kickstart.Services.NetCore.GrpcCommon.Logging;

namespace Kickstart.Services.Startup
{
    public static class ServiceProviderExtensions
    {
        private const int DefaultPort = 50080;
        private static int GetPort(int port)
        {
            if (port > 0) { return port;} else { return DefaultPort;}
        }
        public static Server AddGrpcServices(this IServiceProvider provider, ILoggerFactory loggerFactory, int port)
        {
              
            var servicePort = GetPort(port);
            GrpcEnvironment.SetLogger(new LogLevelFilterLogger(new GrpcLogger(loggerFactory), Grpc.Core.Logging.LogLevel.Debug));
            GrpcEnvironment.Logger.Debug($@"Starting Kickstart services on port {servicePort}");

            var server = new Server
            {
                Services =
                {
                    //Reflection used by some testers
                    KickstartBuildService.BindService(provider.GetRequiredService<KickstartBuildServiceImpl>()),
                    KickstartServiceApi.BindService(provider.GetRequiredService<KickstartServiceImpl>()),

                    ServerReflection.BindService(new ReflectionServiceImpl(KickstartServiceApi.Descriptor, KickstartBuildService.Descriptor,
                    Health.Descriptor))



                },
                Ports =
                {
                    new ServerPort("0.0.0.0", servicePort, ServerCredentials.Insecure)
                 }

            };

            return server;

        }
    }
}
