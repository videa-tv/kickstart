using System.Collections.Generic;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Utility;

namespace Kickstart.Pass2.GrpcServiceProject.Builder
{
    public class ContainerClassBuilder
    {


        public CClass AddContainerClass(KGrpcProject grpcKProject, DataStoreTypes connectsToDatabaseType,
            CProject project,
            CInterface dbProviderInterface,
            CClass dbProviderClass,
            CInterface dbDiagnosticsFactoryInterface,
            CClass dbDiagnosticsFactoryClass)
        {
            var @class = new CClass($"ContainerExtensions")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{grpcKProject.CompanyName}.{grpcKProject.ProjectName}{grpcKProject.NamespaceSuffix}.{grpcKProject.ProjectSuffix}.Infrastructure"
                },
                AccessModifier = CAccessModifier.Public,
                IsStatic = true
            };

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core" } });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.DependencyInjection" }
            });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Configuration" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Options" }
            });

            //@class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "StructureMap" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "MediatR" } });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.GrpcCommon.Infrastructure" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.GrpcCommon.Tracing" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = $"{grpcKProject.CompanyName}.{grpcKProject.ProjectName}{grpcKProject.NamespaceSuffix}.{grpcKProject.ProjectSuffix}.Config" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = dbProviderInterface.Namespace
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = dbDiagnosticsFactoryInterface.Namespace
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = dbDiagnosticsFactoryClass.Namespace
            });

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Lamar" } });





            @class.Method.Add(new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                IsStatic = true,
                IsExtensionMethod = true,
                MethodName = "Configure",
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "ServiceRegistry", ParameterName = "r"},
                    new CParameter { Type = "IConfiguration", ParameterName ="configuration" }
                },
                CodeSnippet = GetConfigureCodeSnippet(grpcKProject.ProjectNameAsClassNameFriendly)
            });
            /*
             @class.Method.Add(new CMethod()
             {
                 AccessModifier = CAccessModifier.Public,
                 ReturnType = "IServiceProvider",
                 MethodName = "GetServiceProvider",
                 CodeSnippet = "return _container.GetInstance<IServiceProvider>();"
             });

             @class.Method.Add(new CMethod
             {
                 AccessModifier = CAccessModifier.Public,
                 ReturnType = "void",
                 MethodName = "ConfigureContainer",

                 CodeSnippet = 
                     $@"_container.Configure(config =>
             {{
                 // Register stuff in container, using the StructureMap APIs
                 // also register MediatR specifics
                 config.Scan(scanner =>
                 {{
                     scanner.AssembliesAndExecutablesFromApplicationBaseDirectory(a => a.FullName.StartsWith(""{
                             grpcKProject.CompanyName
                         }""));
                     scanner.AssemblyContainingType<IMediator>();
                     scanner.WithDefaultConventions();
                     scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                     scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                     scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                     scanner.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                 }});

                 config.For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);

                 config.For<IMediatorExecutor>().Use<MediatorExecutor>();
                 // Populate the container using the service collection
                 config.Populate(_services);
             }});"
             });
             */

            @class.Method.Add(BuildAddAppServicesMethod(grpcKProject, connectsToDatabaseType, dbProviderInterface, dbProviderClass));

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Infrastructure", FileName = $"{@class.ClassName}.cs" }
            });

            return @class;
        }

        private string GetConfigureCodeSnippet(string projectName)
        {
            return
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

                r.AddTransient<IDbDiagnosticsFactory, {projectName}DbDiagnosticsFactory>();
            }}";
        }

        private CMethod BuildAddAppServicesMethod(KGrpcProject grpcKProject,DataStoreTypes connectsToDatabaseType, CInterface dbProviderInterface, CClass dbProviderClass)
        {
            var method = new CMethod
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                IsExtensionMethod = true,
                ReturnType = "void",
                MethodName = "AddAppServices",
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "IServiceCollection", ParameterName = "services"},
                    new CParameter {Type = "IConfiguration", ParameterName = "configuration"}
                }
            };
            var methodSnippet = new CodeWriter();
            methodSnippet.WriteLine($@"services.AddSingleton(");
            methodSnippet.WriteLine($@"p => p.GetRequiredService<IOptions<ServiceSettings>>().Value);");
            methodSnippet.WriteLine();
            methodSnippet.Unindent();


            methodSnippet.WriteLine($@"services.AddSingleton(");
            methodSnippet.Indent();
            methodSnippet.WriteLine($@"p => p.GetRequiredService<IOptions<CloudformationOutputs>>().Value);");
            methodSnippet.Unindent();

            methodSnippet.WriteLine();

            methodSnippet.WriteLine($@"services.AddSingleton(");
            methodSnippet.Indent();
            methodSnippet.WriteLine($@"p => p.GetRequiredService<IOptions<AuthenticationSettings>>().Value);");

            methodSnippet.WriteLine();
            methodSnippet.Unindent();

            if (dbProviderInterface != null)
            {
                methodSnippet.WriteLine($@"services.AddTransient<{dbProviderInterface.InterfaceName}>(");
                methodSnippet.Indent();
                methodSnippet.WriteLine(
                    $@"p => 
                    {{
                        var outputs = p.GetRequiredService<CloudformationOutputs>();");

                methodSnippet.Indent();
                if (connectsToDatabaseType == DataStoreTypes.SqlServer ||
                    connectsToDatabaseType == DataStoreTypes.Postgres ||
                    connectsToDatabaseType == DataStoreTypes.MySql)
                {
                    methodSnippet.WriteLine(
                         $@"if (!string.IsNullOrWhiteSpace(outputs.DBEndpoint))
                        {{
                            return new {grpcKProject.ProjectNameAsClassNameFriendly}DbProvider(
                                outputs.DBEndpoint,
                                outputs.Database,
                                outputs.DBUsername,
                                outputs.DBPassword,
                                outputs.Port);  
                        }}

                        return new {dbProviderClass.ClassName}(configuration.GetConnectionString(""{
                                grpcKProject.ProjectName
                            }""));");
                }
                else
                {
                    methodSnippet.WriteLine($@"return new {dbProviderClass.ClassName}();");
                    methodSnippet.Unindent();
                }

                methodSnippet.WriteLine("});");
                methodSnippet.WriteLine();
                methodSnippet.Unindent();
                methodSnippet.Unindent();
            }

            method.CodeSnippet = methodSnippet.ToString();

            return method;
        }

    }
}