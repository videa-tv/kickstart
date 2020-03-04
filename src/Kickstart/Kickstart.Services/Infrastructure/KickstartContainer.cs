using System;
using Kickstart.Build.Services.Config;
using Kickstart.Interface;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass3.CSharp;
using Kickstart.Pass3.Docker;
using Kickstart.Pass3.VisualStudio2017;
using Kickstart.Services.Config;
using Kickstart.Services.NetCore.GrpcCommon.Infrastructure;
using Kickstart.Utility;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StructureMap;

namespace Kickstart.Services
{
    public class KickstartContainer
    {
        private readonly IContainer _container;
        private readonly IServiceCollection _services;

        public KickstartContainer(IServiceCollection services)
        {
            _services = services;
            _container = new Container();
        }

        public void Configure(IConfigurationRoot configuration)
        {
            _services.AddSingleton(configuration)
                .AddLogging()
                .AddOptions()
                .Configure<ServiceSettings>(configuration)
                //.Configure<CloudformationOutputs>(configuration.GetSection("CloudformationOutputs"))
                .Configure<AuthenticationSettings>(configuration.GetSection("Authentication"));

            AddAppServices(configuration);
            /*
            var tracingOptions = new TracingOptions(configuration);
            if (tracingOptions.Enabled)
            {
                _services.AddDatadogOpenTracing(new TracingSettings
                {
                    TracingAddress = tracingOptions.TracingAddress,
                    ServiceName = tracingOptions.ServiceName
                });
            }

            _services.AddTransient<IDbDiagnosticsFactory>(serviceProvider => new DbDiagnosticsFactory(serviceProvider, tracingOptions.Enabled));
            */
            ConfigureContainer(configuration);
        }

        public void ConfigureContainer(IConfigurationRoot configurationRoot)
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
                    scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                });

                config.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
                
                config.For(typeof(IFileWriter)).Use(new FileWriter());
                config.For(typeof(ICInterfaceVisitor)).Use(typeof(CSharpCInterfaceVisitor));
                config.For(typeof(ICClassVisitor)).Use(typeof(CSharpCClassVisitor));
                config.For(typeof(ICMethodVisitor)).Use(typeof(CSharpCMethodVisitor));
                config.For(typeof(ICPropertyVisitor)).Use(typeof(CSharpCPropertyVisitor));
                config.For(typeof(ICParameterVisitor)).Use(typeof(CSharpCParameterVisitor));
                config.For(typeof(ICFieldVisitor)).Use(typeof(CSharpCFieldVisitor));
                config.For(typeof(ICAssemblyInfoVisitor)).Use(typeof(CSharpCAssemblyInfoVisitor));
                config.For(typeof(ICEnumVisitor)).Use(typeof(CSharpCEnumVisitor));

                config.For(typeof(ICClassAttributeVisitor)).Use(typeof(CSharpCClassAttributeVisitor));
                config.For(typeof(ICConstructorVisitor)).Use(typeof(CSharpCConstructorVisitor));
                config.For(typeof(ICMethodAttributeVisitor)).Use(typeof(CSharpCMethodAttributeVisitor));
                config.For(typeof(ICDockerFileServiceVisitor)).Use(typeof(CDockerFileServiceVisitor));
                config.For(typeof(ICSolutionVisitor)).Use(typeof(FastSolutionVisitor));
                config.For(typeof(IGrpcServiceProjectService)).Use(typeof(GrpcServiceProjectService));
                config.For(typeof(IGrpcIntegrationServiceProjectService)).Use(typeof(GrpcIntegrationServiceProjectService));
                //config.For(typeof(IGrpcServiceClientTestProjectService)).Use(typeof(GrpcServiceClientTestProjectService));
                config.For(typeof(IGrpcPortService)).Use(new GrpcPortService());

                config.For<IMediatorExecutor>().Use<MediatorExecutor>();
                config.For<IMediator>().Use<Mediator>();
                config.ForSingletonOf<IConfigurationRoot>().Use(configurationRoot);

                config.ForSingletonOf<IServiceSettings>().Use(c => c.GetInstance<IOptions<ServiceSettings>>().Value);

                //config.For<IDataLayerServiceProjectService>().AddInstance()..Use<RelationalDataLayerServiceProjectService>();
                // Populate the container using the service collection
                config.Populate(_services);
            });

        }

        public void AddAppServices(IConfiguration configuration)
        {

        }

        public IServiceProvider GetServiceProvider()
        {
            return _container.GetInstance<IServiceProvider>();
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }


}