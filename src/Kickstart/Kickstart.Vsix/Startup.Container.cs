using System;
using Kickstart.Interface;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass3.CSharp;
using Kickstart.Pass3.Docker;
using Kickstart.Pass3.VisualStudio2017;
using Kickstart.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StructureMap;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Kickstart.Vsix
{
    public static class ServiceCollectionAppServicesExtensions
    {
        public static IServiceProvider ConfigureContainer(this IServiceCollection services)
        {
            var container = new Container();
            container.Configure(config =>
            {
                // Register stuff in container, using the StructureMap APIs
                // also register MediatR specifics
                config.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<ILogger>();
                    scanner.AssemblyContainingType<IKickstartService>();
                    //scanner.AssembliesAndExecutablesFromApplicationBaseDirectory(a => a.FullName.StartsWith("Kickstart"));
                    scanner.SingleImplementationsOfInterface();
                    //scanner.WithDefaultConventions();
                   
                });
                config.For(typeof(IFileWriter)).Use(new FileWriter());
                config.For(typeof(ICInterfaceVisitor)).Use(typeof(CSharpCInterfaceVisitor));
                config.For(typeof(ICClassVisitor)).Use(typeof(CSharpCClassVisitor));
                config.For(typeof(ICEnumVisitor)).Use(typeof(CSharpCEnumVisitor));

                config.For(typeof(ICMethodVisitor)).Use(typeof(CSharpCMethodVisitor));
                config.For(typeof(ICPropertyVisitor)).Use(typeof(CSharpCPropertyVisitor));
                config.For(typeof(ICParameterVisitor)).Use(typeof(CSharpCParameterVisitor));
                config.For(typeof(ICFieldVisitor)).Use(typeof(CSharpCFieldVisitor));
                config.For(typeof(ICAssemblyInfoVisitor)).Use(typeof(CSharpCAssemblyInfoVisitor));

                config.For(typeof(ICClassAttributeVisitor)).Use(typeof(CSharpCClassAttributeVisitor));
                config.For(typeof(ICConstructorVisitor)).Use(typeof(CSharpCConstructorVisitor));
                config.For(typeof(ICMethodAttributeVisitor)).Use(typeof(CSharpCMethodAttributeVisitor));
                config.For(typeof(ICDockerFileServiceVisitor)).Use(typeof(CDockerFileServiceVisitor));
                config.For(typeof(ICSolutionVisitor)).Use(typeof(FastSolutionVisitor));
                config.For(typeof(IGrpcServiceProjectService)).Use(typeof(GrpcServiceProjectService));
                config.For(typeof(IGrpcIntegrationServiceProjectService)).Use(typeof(GrpcIntegrationServiceProjectService));

                // Populate the container using the service collection
                config.Populate(services);
            });

            return container.GetInstance<IServiceProvider>();
        }

	}
}
