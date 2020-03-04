using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface IStartupContainerClassBuilder
    {
        CClass BuildStartupContainerClass(KDataStoreTestProject sqlTestKProject, CInterface dbProviderInterface, CClass dbProviderClass);
    }

    public class StartupContainerClassBuilder : IStartupContainerClassBuilder
    {
        public CClass BuildStartupContainerClass(KDataStoreTestProject sqlTestKProject, CInterface dbProviderInterface, CClass dbProviderClass)
        {
            var @class = new CClass("ServiceCollectionExtensions")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                       $"{sqlTestKProject.CompanyName}.{sqlTestKProject.ProjectName}{sqlTestKProject.NamespaceSuffix}.{sqlTestKProject.ProjectSuffix}.Startup"
                },

                AccessModifier = CAccessModifier.Public,
                IsStatic = true
            };

           
            AddNamespaceRefs(@class, dbProviderInterface);

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                FieldType = "IContainer",
                FieldName = "_container"
            });

            @class.Method.Add(new CMethod
            {
                IsStatic = true,
                IsExtensionMethod = true,
                ReturnType = "IServiceProvider",
                MethodName = "ConfigureContainer",
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "IServiceCollection", ParameterName = "services"}
                },
                CodeSnippet = 
          $@"
            r.Scan(scanner =>
            {{
                scanner.AssembliesAndExecutablesFromApplicationBaseDirectory(a => a.FullName.StartsWith(""Company""));
                scanner.WithDefaultConventions();
                scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
            }});

            r.For<IMediator>().Use<Mediator>().Transient();
            r.For<ServiceFactory>().Use(ctx => ctx.GetInstance);
            r.For<IMediatorExecutor>().Use(ctx => new MediatorExecutor(ctx.GetInstance<IServiceScopeFactory>()));

            r.AddAppServices(configuration);

            r.AddSingleton(configuration)
                .AddLogging()
                .AddOptions()
                .Configure<CloudformationOutputs>(configuration.GetSection(""CloudformationOutputs""))
                .Configure<AuthenticationSettings>(configuration.GetSection(""Authentication""));

            var tracingOptions = new TracingOptions(configuration);
            if (tracingOptions.Enabled)
            {{
                r.AddDatadogOpenTracing(new TracingSettings
                {{
                    TracingAddress = tracingOptions.TracingAddress,
                    ServiceName = tracingOptions.ServiceName
                }});

                r.AddTransient<IDbDiagnosticsFactory, ServiceDbDiagnosticsFactory>();
            }}
        "
            });

            @class.Method.Add(
                new CMethod
                {
                    AccessModifier = CAccessModifier.Public,
                    IsStatic = true,
                    IsExtensionMethod = true,
                    ReturnType = "void",
                    MethodName = "Dispose",
                    Parameter = new List<CParameter>
                    {
                        new CParameter {Type = "IServiceProvider", ParameterName = "services"}
                    },
                    CodeSnippet = "_container.Dispose();"
                });


            return @class;
        }
        private void AddNamespaceRefs(CClass @class, CInterface dbProviderInterface)
        {
            var namespaces = new List<string>
            {
                "System",
                "Microsoft.Extensions.DependencyInjection",
                //"StructureMap",
                "Company.Datastore",
                "Company.Datastore.Connection",
                "Company.Datastore.Dapper",
                $"{dbProviderInterface.Namespace.NamespaceName}"

            };

            foreach (var ns in namespaces)
            {
                @class.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = ns }
                });
            }
        }
    }
}
