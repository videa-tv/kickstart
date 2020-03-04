using System;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StructureMap;
using MediatR;
using Kickstart.Build.Services.Config;
using Kickstart.Build.Services.GrpcCommon.Infrastructure;

namespace Kickstart.Build.Services.Startup
{
    public class BuildContainer
    {
        private readonly IServiceCollection _services;
        private readonly IContainer _container;

        public BuildContainer(IServiceCollection services)
        {
            _services = services;
            _container = new Container();

        }

        public void Configure(IConfiguration configuration)
        {
            _services.AddSingleton(configuration)
                .AddLogging()
                .AddOptions()
                .Configure<CloudformationOutputs>(configuration.GetSection("CloudformationOutputs"))
                .Configure<AuthenticationSettings>(configuration.GetSection("Authentication"));

            AddAppServices(configuration);

            /*
            var tracingOptions = new TracingOptions(configuration);
            if (tracingOptions.Enabled)
            {
                _services.AddDatadogTracing(new TracingSettings
                {
                    TracingAddress = tracingOptions.TracingAddress,
                    ServiceName = tracingOptions.ServiceName
                });

                //_services.AddTransient<IDbDiagnosticsFactory, BuildDbDiagnosticsFactory>();
            }*/

            ConfigureContainer();

        }

        public void ConfigureContainer()
        {
            _container.Configure(config =>
            {
                // Register stuff in container, using the StructureMap APIs
                // also register MediatR specifics
                config.Scan(scanner =>
                {
                    scanner.AssembliesAndExecutablesFromApplicationBaseDirectory(a => a.FullName.StartsWith("Kickstart"));
                    scanner.AssemblyContainingType<IMediator>();
                    scanner.WithDefaultConventions();
                    scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                });

                config.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                config.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));

                config.For<IMediatorExecutor>().Use<MediatorExecutor>();
                // Populate the container using the service collection
                config.Populate(_services);
            });

        }

        public IServiceProvider GetServiceProvider()
        {
            return _container.GetInstance<IServiceProvider>();

        }

        public void AddAppServices(IConfiguration configuration)
        {
            _services.AddSingleton(
            p => p.GetRequiredService<IOptions<ServiceSettings>>().Value);


            _services.AddSingleton(
            p => p.GetRequiredService<IOptions<CloudformationOutputs>>().Value);


            _services.AddSingleton(
            p => p.GetRequiredService<IOptions<AuthenticationSettings>>().Value);

            
        }

        public void Dispose()
        {
            _container.Dispose();

        }

    }

}
