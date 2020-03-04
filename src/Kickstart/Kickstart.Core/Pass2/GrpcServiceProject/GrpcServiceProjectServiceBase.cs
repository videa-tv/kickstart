using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.DataLayerProject.Table;
using Kickstart.Utility;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.DataLayerProject;
using Kickstart.Pass2.GrpcServiceProject.Builder;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public class GrpcServiceProjectServiceBase
    {
        private KDataStoreProject _dataStoreKProject;
        private CProject _dataStoreProject;

        private KDataLayerProject _dataLayerKProject;
        private CProject _dataLayerProject;

        protected KGrpcProject _grpcKProject;


        protected KSolution _kSolution;
 
        private readonly IConfigurationRoot _configuration;
        private readonly IGrpcPortService _grpcPortService;
        private readonly ICTableToCClassConverter _cTableToCClassConverter;
        private readonly IEntityToModelCClassConverter _entityToModelCClassConverter;
        private readonly IModelToEntityCClassConverter _modelToEntityCClassConverter;
        private readonly IModelToProtoCClassConverter _modelToProtoCClassConverter;

        private readonly ICProjectToDockerFileConverter _cProjectToDockerFileConverter;
        private readonly IKDataLayerProjectToKProtoFileConverter _kDataLayerProjectToKProtoFileConverter;
        protected readonly IServiceImplClassBuilder _serviceImplClassBuilder;
        private readonly ContainerClassBuilder _containerClassBuilder;
        
        public GrpcServiceProjectServiceBase(IConfigurationRoot configuration, IGrpcPortService grpcPortService, ICTableToCClassConverter cTableToCClassConverter, IEntityToModelCClassConverter entityToModelCClassConverter, IModelToProtoCClassConverter modelToProtoCClassConverter, ICProjectToDockerFileConverter cProjectToDockerFileConverter, IKDataLayerProjectToKProtoFileConverter kDataLayerProjectToKProtoFileConverter, IServiceImplClassBuilder serviceImplClassBuilder, IModelToEntityCClassConverter modelToEntityCClassConverter)
        {
            _configuration = configuration;
            _grpcPortService = grpcPortService;

            _cTableToCClassConverter = cTableToCClassConverter;
            _entityToModelCClassConverter = entityToModelCClassConverter;
            _modelToProtoCClassConverter = modelToProtoCClassConverter;
            _cProjectToDockerFileConverter = cProjectToDockerFileConverter;
            _kDataLayerProjectToKProtoFileConverter = kDataLayerProjectToKProtoFileConverter;
            _serviceImplClassBuilder = serviceImplClassBuilder;
            _modelToEntityCClassConverter = modelToEntityCClassConverter;
            _containerClassBuilder = new ContainerClassBuilder();
        }
        public virtual CProject BuildProject(KSolution kSolution, KDataStoreProject databaseKProject,
            KDataLayerProject dataLayerKProject, KGrpcProject grpcKProject, CProject sqlProject, CProject dataLayerProject)
        {
            _dataStoreKProject = databaseKProject;
            _dataLayerKProject = dataLayerKProject;
            _kSolution = kSolution;
            _grpcKProject = grpcKProject;
            _dataStoreProject = sqlProject;
            _dataLayerProject = dataLayerProject;

            var project = new CProject
            {
                ProjectName = grpcKProject.ProjectFullName,
                ProjectShortName = grpcKProject.ProjectShortName,
                ProjectFolder = grpcKProject.ProjectFolder,
                ProjectType = CProjectType.CsProj
            };
            project.ProjectReference.Add(dataLayerProject);
            if (grpcKProject.DotNetType == Pass1.KModel.Project.DotNetType.Framework)
            {
                project.TemplateProjectPath = @"templates\NetFrameworkClassLibrary.csproj";

            }
            else
            {
                project.TemplateProjectPath = @"templates\NetCore31ConsoleApp.csproj";
            }

            CInterface dataServiceInterface = _dataLayerProject?.ProjectContent.FirstOrDefault(pc =>
                    pc.Content is CInterface && (pc.Content as CInterface).InterfaceName.EndsWith("DataService"))
                ?.Content as CInterface;
            var dbProviderClass = _dataLayerProject?.ProjectContent.First(pc =>
                pc.Content is CClass && (pc.Content as CClass).ClassName.EndsWith("DbProvider")).Content as CClass;
            var dbProviderInterface = _dataLayerProject?.ProjectContent.First(pc =>
                    pc.Content is CInterface && (pc.Content as CInterface).InterfaceName.EndsWith("DbProvider"))
                .Content as CInterface;


            //AddLoggerClass(project);//use Nuget instead
            //AddGitIgnoreFile(project);
            //AddProtoBatchFile(project);
            AddAppClass(project);
            //AddLogEvents(project);
            //AddGraceful(project);
            

            var iDbDiagnosticsFactoryInterface =  _dataLayerProject.Interface.FirstOrDefault(i => i.InterfaceName == "IDbDiagnosticsFactory");

            CClass classDbDiagnosticsFactory = null;

            if (!(_dataStoreProject is CNullProject))
            {
                AddDbDiagnosticsHandler(project);
                classDbDiagnosticsFactory = AddDbDiagnosticsFactory(project, iDbDiagnosticsFactoryInterface);
            }
            var containerClass = _containerClassBuilder.AddContainerClass(_grpcKProject, _dataLayerKProject.ConnectsToDatabaseType, project, dbProviderInterface, dbProviderClass, iDbDiagnosticsFactoryInterface, classDbDiagnosticsFactory);
            //AddServiceProviderFactory(project, containerClass);

            //AddLoggingInterceptor(project); //available in base Nuget library
            AddAuthExtensions(project);
            AddAuthenticationSettings(project);
            AddCloudformationOutputs(project);

            if (_grpcKProject is KGrpcIntegrationProject)
            {
                AddApiHosts(project);
                AddLimitsSettings(project);

            }

            AddServiceSettings(project);
            AddTracingOptions(project);

            AddVersionTxt(project);
            //AddStartupAppServices(project, dbProviderClass, dbProviderInterface);

            //use Nuget instead 
            //todo: use metadata setting, want option of not depending on 3rd party nuget
            //var mediatorInterface = AddIMediatorInterface(project);
            //AddMediatorClass(project, mediatorInterface);
            AddDockerFile(project);
            AddNugetRefs(project);


            if (dataLayerKProject != null)
            {
                var mProtoFiles = BuildProtoFile(databaseKProject, dataLayerKProject, grpcKProject);
                /*
                foreach (var m in mProtoFiles)
                {
                    grpcKProject.ProtoFile.Add(m);
                }*/
            }
            AddAppSettingsJson(project, grpcKProject.ProtoFile.FirstOrDefault()?.GeneratedProtoFile);
            foreach (var kProtoFile in grpcKProject.ProtoFile)
            {
                var protoFile = kProtoFile.GeneratedProtoFile;
                kProtoFile.GeneratedProtoFile = protoFile;
                AddProtoFile(project, protoFile);
                AddEnums(project, databaseKProject, dataLayerProject, protoFile);
                AddModelClasses(project, databaseKProject, dataLayerProject, protoFile);
                
                AddDomainModelExtensionClass(project, dataLayerProject, protoFile, protoFile.CSharpNamespace);
                
                AddQuery(project, sqlProject, protoFile);
                AddHandlers(project, sqlProject, dataServiceInterface, null, protoFile);
                foreach (var service in kProtoFile.GeneratedProtoFile.ProtoService)
                {
                    var implClass = _serviceImplClassBuilder.BuildServiceImplClass( _grpcKProject, service, protoFile.CSharpNamespace);
                    project.ProjectContent.Add(new CProjectContent
                    {
                        Content = implClass,
                        BuildAction = grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                        File = new CFile { Folder = $@"Implementation", FileName = $"{implClass.ClassName}.cs" }
                    });
                }

            }
            AddStartupGrpcClass(project, grpcKProject.ProtoFile);
            AddHealthServiceImpl(project);
            AddHealthCheckQuery(project);
            AddHealthCheckHandler(project);
            AddProgramClass(project, containerClass);

            return project;
        }

        
        /*
        private void AddServiceProviderFactory(CProject project, CClass containerClass)
        {
            var @class = new CClass($"{_grpcKProject.ProjectNameAsClassNameFriendly}ServiceProviderFactory")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Infrastructure"
                }
            };

            @class.Implements.Add(new CInterface() { InterfaceName = $"IServiceProviderFactory<{containerClass.ClassName}>" });

            @class.Implements.Add(new CInterface() { InterfaceName = "IDisposable" });

            @class.NamespaceRef.Add(new CNamespaceRef("System"));

            @class.NamespaceRef.Add(new CNamespaceRef("Microsoft.Extensions.DependencyInjection"));

            @class.NamespaceRef.Add(new CNamespaceRef(containerClass.Namespace) );

            @class.Field.Add(new CField() { FieldType = containerClass.ClassName, FieldName = "_container" });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = containerClass.ClassName,
                MethodName = "CreateBuilder",
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = "IServiceCollection", ParameterName = "services"}
                },
                CodeSnippet = 
                  $@"_container = new {_grpcKProject.ProjectNameAsClassNameFriendly}Container(services);
                    return _container;"
            });

            @class.Method.Add(new CMethod()
            {
                ReturnType  = "IServiceProvider", MethodName = "CreateServiceProvider",
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = containerClass.ClassName, ParameterName = "containerBuilder"}
                },
                CodeSnippet = "return containerBuilder.GetServiceProvider();"
            });

            @class.Method.Add(new CMethod()
            {
                ReturnType = "void", MethodName = "Dispose",
                CodeSnippet = "_container?.Dispose();"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Infrastructure", FileName = $"{@class.ClassName}.cs" }
            });

        }*/

        private void AddDbDiagnosticsHandler(CProject project)
        {
            var @class = new CClass($"{_grpcKProject.ProjectNameAsClassNameFriendly}DbDiagnosticsHandler")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Infrastructure"
                }
            };
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace() { NamespaceName = "System" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace() {NamespaceName = "Company.GrpcCommon.Tracing"}
            });

            var iDbDiagnosticsHandlerInterface =
                _dataLayerProject.Interface.FirstOrDefault(i => i.InterfaceName == "IDbDiagnosticsHandler");
            
            @class.NamespaceRef.Add(new CNamespaceRef(iDbDiagnosticsHandlerInterface.Namespace));
            @class.Implements.Add(iDbDiagnosticsHandlerInterface);

            @class.Constructor.Add(new CConstructor()
            {
                ConstructorName = @class.ClassName,
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = "ITracerContext", ParameterName = "tracerContext"},
                    new CParameter() { Type = "string", ParameterName = "name"},
                   
                },
                CodeSnippet = "_traceHandler = tracerContext.GetTraceHandler(\"sql\", name);"

            });

            @class.Field.Add(new CField { AccessModifier = CAccessModifier.Private, IsReadonly = true, FieldType = "ITraceHandler", FieldName = "_traceHandler" });

            @class.Method.Add(new CMethod()
            {
                UseExpressionDefinition = true,
                AccessModifier = CAccessModifier.Public, ReturnType = "void", MethodName = "Start",
                CodeSnippet = "_traceHandler.Start();"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public, ReturnType = "void", MethodName = "Complete",
                CodeSnippet = "_traceHandler.Complete();"
            });
            @class.Method.Add(new CMethod()
                {
                    AccessModifier = CAccessModifier.Public,
                    ReturnType = "void",
                    MethodName = "CompleteError",
                    Parameter = new List<CParameter>()
                    {
                        new CParameter()
                        {
                            Type = "Exception",
                            ParameterName = "ex"
                        }
                    },
                    CodeSnippet = "_traceHandler.CompleteError(ex);"
            }
            );

            @class.Method.Add(new CMethod()
            {
                UseExpressionDefinition = false,
                ReturnType = "IDbDiagnosticsHandler",
                MethodName = "WithTag",
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = "string", ParameterName = "key"},
                    new CParameter() { Type = "string", ParameterName = "value"}
                },
                CodeSnippet = 
                      @"_traceHandler.AddTag(key, value);
                        return this; "
            });
            
            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Infrastructure", FileName = $"{@class.ClassName}.cs" }
            });

        }

        private CClass AddDbDiagnosticsFactory(CProject project, CInterface iDbDiagnosticsFactoryInterface)
        {
            var @class = new CClass($"{_grpcKProject.ProjectNameAsClassNameFriendly}DbDiagnosticsFactory")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Infrastructure"
                }
            };

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace() { NamespaceName = "Company.GrpcCommon.Tracing" } });

            var iDbDiagnosticsHandlerInterface =
                _dataLayerProject.Interface.FirstOrDefault(i => i.InterfaceName == "IDbDiagnosticsHandler");

            @class.Implements.Add(iDbDiagnosticsFactoryInterface);

            @class.NamespaceRef.Add(new CNamespaceRef( iDbDiagnosticsFactoryInterface.Namespace));
            @class.NamespaceRef.Add(new CNamespaceRef(iDbDiagnosticsHandlerInterface.Namespace));

            @class.Field.Add(new CField() { FieldType = "ITracerContext", FieldName = "_tracerContext", IsReadonly = true });

            @class.Constructor.Add(new CConstructor()
            {
                ConstructorName =  @class.ClassName,
                CodeSnippet = "_tracerContext = tracerContext;",
                Parameter = new List<CParameter>()
                {
                    new CParameter() {Type = "ITracerContext", ParameterName = "tracerContext"}
                }
            });

            @class.Method.Add(new CMethod()
            {
                UseExpressionDefinition = true,
                ReturnType = "IDbDiagnosticsHandler",
                MethodName = "CreateHandler",
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = "string", ParameterName = "name"}
                },
                CodeSnippet = $"new {_grpcKProject.ProjectNameAsClassNameFriendly}DbDiagnosticsHandler(_tracerContext, name);"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Infrastructure", FileName = $"{@class.ClassName}.cs" }
            });

            return @class;
        }


        private IList<CClass> BuildModelClasses(KDataStoreProject databaseKProject, CProject dataLayerProject, CProtoFile protoFile)
        {
            var modelClasses = new List<CClass>();
            /*
            var viewConverter = new CViewToCClassConverter();
            foreach (var view in databaseKProject.View)
            {
                var modelClass = viewConverter.Convert(view.GeneratedView);
                modelClass.Namespace = new CNamespace
                {
                    NamespaceName =
                      $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Model"
                };
                modelClass.DerivedFrom = view;
                modelClasses.Add(modelClass);
            }*/

            foreach (var table in databaseKProject.Table)
            {
                
                var modelClass = _cTableToCClassConverter.Convert(table.GeneratedTable, databaseKProject.Table.Select(kt=>kt.GeneratedTable), true);
                modelClass.Namespace = new CNamespace
                {
                    NamespaceName =
                      $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Model"
                };

                modelClass.DerivedFrom =  table;

                modelClasses.Add(modelClass);
            }

            return modelClasses;
        }

        private IList<CEnum> BuildEnums(KDataStoreProject databaseKProject, CProject dataLayerProject, CProtoFile protoFile)
        {
            var enums = new List<CEnum>();

            foreach (var protoEnum in protoFile.ProtoEnum)
            {
                var converter = new GrpcEnumToEnumConverter();
                var enumItem = converter.Convert(protoEnum);
                enumItem.Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Model"
                };
                enumItem.DerivedFrom = protoEnum;
                enums.Add(enumItem);
            }

            return enums;
        }


        //assume Service model is 1 to 1 with table structure (should this be DAL entities? )
        private void AddModelClasses(CProject project, KDataStoreProject databaseKProject, CProject dataLayerProject, CProtoFile protoFile)
        {    
            var modelClasses = BuildModelClasses(databaseKProject, dataLayerProject, protoFile);
            foreach (var modelClass in modelClasses)
            {
            
                project.ProjectContent.Add(new CProjectContent
                {
                    Content = modelClass,
                    BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                    File = new CFile { Folder = $@"Model", FileName = $"{modelClass.ClassName}.cs" }
                });
            }

        }

        private void AddEnums(CProject project, KDataStoreProject databaseKProject, CProject dataLayerProject, CProtoFile protoFile)
        {
            var enums = BuildEnums(databaseKProject, dataLayerProject, protoFile);
            foreach (var e in enums)
            {

                project.ProjectContent.Add(new CProjectContent
                {
                    Content = e,
                    BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                    File = new CFile { Folder = $@"Model", FileName = $"{e.EnumName}.cs" }
                });


            }

            //throw new NotImplementedException();
            //AddEntityToModelClass(project, dataLayerProject, modelClasses, protoFile, protoFile.CSharpNamespace);
        }
        
        public void AddVersionTxt(CProject project)
        {
            var text = new CText();
            text.Text = "1.0.0";

            project.ProjectContent.Add(new CProjectContent
            {
                Content = text,
                BuildAction = CBuildAction.None,
                CopyToOutputDirectory = true,
                CopyToPublishDirectory = true,
                File = new CFile { Folder = $@"", FileName = $"version.txt", Encoding = Encoding.ASCII }
            });
        }
        public void AddGraceful(CProject project)
        {
            var @class = new CClass ($"Graceful")
            {
                IsStatic = true,
                Namespace = new CNamespace
                {
                    NamespaceName =
                   $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Infrastructure"
                }
            };
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Runtime.InteropServices" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Mono.Unix" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Mono.Unix.Native" } });

            //@class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Infrastructure" } });

            @class.Method.Add(new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                IsStatic = true,
                ReturnType = "void",
                MethodName = "OnShutdown",
                Parameter = new List<CParameter> { new CParameter { Type ="Action", ParameterName = "action" } },
                CodeSnippet =
               @"// handle sigterm on non-windows
                if (!IsWindows())
                {
                    var signalRaised = new UnixSignal(Signum.SIGTERM);
                    while (signalRaised.WaitOne())
                    {
                        action?.Invoke();
                    }
                }"
            });

            @class.Method.Add(new CMethod
            {
                UseExpressionDefinition = true,
                AccessModifier = CAccessModifier.Private, IsStatic = true, ReturnType = "bool", MethodName = "IsWindows",
                CodeSnippet = "RuntimeInformation.IsOSPlatform(OSPlatform.Windows);"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Infrastructure", FileName = $"{@class.ClassName}.cs" }
            });
        }

        public void AddLoggingInterceptor(CProject project)
        {
            var @class = new CClass($"LoggingInterceptor")
            {
                IsStatic = false,
                InheritsFrom = new CClass ("Interceptor" ),
                Namespace = new CNamespace
                {
                    NamespaceName =
                   $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Interceptors"
                }
            };
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core.Interceptors" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Logging" } });
            //@class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Logging.Internal" } });

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Infrastructure" } });


            @class.Field.Add(new CField { AccessModifier = CAccessModifier.Private, IsReadonly = true, FieldType = "ILogger<LoggingInterceptor>", FieldName = "_logger" });
            @class.Field.Add(new CField { AccessModifier = CAccessModifier.Private, IsReadonly = true, FieldType = "LogLevel", FieldName = "_logLevel" });

            @class.Constructor.Add(new CConstructor
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = "LoggingInterceptor",
                Parameter = new List<CParameter>
                {
                    new CParameter { Type = "ILogger<LoggingInterceptor>", ParameterName = "logger" },
                    new CParameter { Type = "LogLevel", ParameterName = "logLevel" }
                },
                CodeSnippet =
              @"_logger = logger;
                _logLevel = logLevel;"
            });

            @class.Method.Add(new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                IsOverride = true,
                IsAsync = true,
                ReturnType = "Task<TResponse>",
                MethodName = "UnaryServerHandler<TRequest, TResponse>",
                Parameter = new List<CParameter>
                {
                    new CParameter { Type = "TRequest", ParameterName = "request" },
                    new CParameter { Type = "ServerCallContext", ParameterName = "context" },
                    new CParameter { Type = "UnaryServerMethod<TRequest, TResponse>", ParameterName = "continuation" }

                },
                CodeSnippet =
              @"var requestId = Guid.NewGuid().ToString(""N"");

                Log(""Start calling {0}: {1}. RequestId: {3}"", context.Method, request, requestId);

                var result = await continuation(request, context).ConfigureAwait(false);

                Log(""End calling {0}. RequestId: {1}"", context.Method, requestId);

                return result; "
            });

            @class.Method.Add(new CMethod
            {
                AccessModifier = CAccessModifier.Private,
                ReturnType = "void",
                MethodName = "Log",
                Parameter = new List<CParameter>
                {
                    new CParameter { Type = "string", ParameterName = "message" },

                    new CParameter { Type = "params object[]", ParameterName = "args" }
                },
                CodeSnippet = @"_logger.Log(_logLevel, LogEvents.Generic, new FormattedLogValues(message, args), null, MessageFormatter);"
            });

            @class.Method.Add(new CMethod
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                ReturnType = "string",
                MethodName = "MessageFormatter",
                Parameter = new List<CParameter>
                {
                    new CParameter { Type = "object", ParameterName = "state" },
                    new CParameter { Type = "Exception", ParameterName = "error"}
                },
                CodeSnippet = "return state.ToString();"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Interceptors", FileName = $"{@class.ClassName}.cs" }
            });
        }
        public void AddAuthExtensions(CProject project)
        {
            var @class = new CClass ($"AuthExtensions")
            {
                IsStatic = true,
                Namespace = new CNamespace
                {
                    NamespaceName =
                      $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Auth"
                }
            };
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Linq" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Config" } });


            @class.Field.Add(new CField { AccessModifier = CAccessModifier.Public, IsStatic = true,  IsReadonly = true, FieldType = "string", FieldName = "ApiKey", DefaultValue = @"""api_key""" });
            @class.Field.Add(new CField { AccessModifier = CAccessModifier.Private, IsConst=true, IsStatic = false, IsReadonly = false, FieldType = "string", FieldName = "ErrorMessage", DefaultValue = @"""Please provide a valid api_key""" });

            @class.Method.Add(new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                IsStatic = true,
                ReturnType = "void",
                MethodName = "CheckAuthenticated",
                IsExtensionMethod = true,
                Parameter = new List<CParameter>
                {
                    new CParameter { Type = "ServerCallContext", ParameterName = "context" },
                    new CParameter { Type = "AuthenticationSettings", ParameterName = "authSettings" }
                },
                CodeSnippet =
                @"if (!authSettings.Enabled)
                {
                    return;
                }

                var apiKey =
                    context.RequestHeaders.FirstOrDefault(h => h.Key.Equals(ApiKey, StringComparison.OrdinalIgnoreCase));

                if (apiKey == null || !apiKey.Value.Equals(authSettings.ApiKey, StringComparison.OrdinalIgnoreCase))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, ErrorMessage));
                }"

            });
            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Auth", FileName = $"{@class.ClassName}.cs" }
            });
        }
        public void AddAuthenticationSettings(CProject project)
        {
            var @class = new CClass ($"AuthenticationSettings")
            {
                IsStatic = false,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Config"
                }
            };

            @class.Property.Add(new CProperty { Type = "bool", PropertyName = "Enabled" });
            @class.Property.Add(new CProperty { Type = "string", PropertyName = "ApiKey" });
     
            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Config", FileName = $"{@class.ClassName}.cs" }
            });
        }
        public void AddCloudformationOutputs(CProject project)
        {
            var @class = new CClass($"CloudformationOutputs")
            {
                IsStatic = false,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Config"
                }
            };
            //@class.NamespaceRef.Add(new SNamespaceRef { ReferenceTo = new SNamespace { NamespaceName = "Microsoft.Extensions.Logging" } });
            @class.Property.Add(new CProperty { Type ="string", PropertyName = "DBEndpoint" });
            @class.Property.Add(new CProperty { Type = "string", PropertyName = "Database" });
            @class.Property.Add(new CProperty { Type = "string", PropertyName = "DBUsername" });
            @class.Property.Add(new CProperty { Type = "string", PropertyName = "DBPassword" });
            @class.Property.Add(new CProperty { Type = "int", PropertyName = "Port" });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Config", FileName = $"{@class.ClassName}.cs" }
            });
        }

        public void AddApiHosts(CProject project)
        {
            var @class = new CClass ($"ApiHosts")
            {
                IsStatic = false,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Config"
                }
            };
            //@class.NamespaceRef.Add(new SNamespaceRef { ReferenceTo = new SNamespace { NamespaceName = "Microsoft.Extensions.Logging" } });


            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Config", FileName = $"{@class.ClassName}.cs" }
            });
        }

        public void AddLimitsSettings(CProject project)
        {
            var @class = new CClass ($"LimitsSettings")
            {
                IsStatic = false,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Config"
                }
            };
            //@class.NamespaceRef.Add(new SNamespaceRef { ReferenceTo = new SNamespace { NamespaceName = "Microsoft.Extensions.Logging" } });

            @class.Property.Add(new CProperty
            {
                AccessModifier = CAccessModifier.Public,
                Type = "int",
                PropertyName = "MaxPageSize"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Config", FileName = $"{@class.ClassName}.cs" }
            });
        }

        public void AddTracingOptions(CProject project)
        {
            var @class = new CClass($"TracingOptions")
            {
                IsStatic = false,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Config"
                }
            };

            @class.NamespaceRef.Add(new CNamespaceRef() { ReferenceTo = new CNamespace() { NamespaceName = "Microsoft.Extensions.Configuration" } });

            @class.Constructor.Add(new CConstructor()
            {
                ConstructorName = "TracingOptions",
                Parameter = new List<CParameter>()
                {
                    new CParameter()
                    {
                        Type = "IConfiguration",
                        ParameterName = "configuration"
                    }
                },
                CodeSnippet = "configuration.GetSection(\"Tracing\").Bind(this);"
            });

            @class.Property.Add(new CProperty
            {
                AccessModifier = CAccessModifier.Public,
                Type = "bool",
                PropertyName = "Enabled"
            });

            @class.Property.Add(new CProperty
            {
                AccessModifier = CAccessModifier.Public,
                Type = "bool",
                PropertyName = "Verbose"
            });

            @class.Property.Add(new CProperty
            {
                AccessModifier = CAccessModifier.Public,
                Type = "string",
                PropertyName = "ServiceName"
            });

            @class.Property.Add(new CProperty
            {
                AccessModifier = CAccessModifier.Public,
                Type = "string",
                PropertyName = "TracingAddress"
            });

            
            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Config", FileName = $"{@class.ClassName}.cs" }
            });
        }


        public void AddServiceSettings(CProject project)
        {
            var @class = new CClass ($"ServiceSettings")
            {
                IsStatic = false,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Config"
                }
            };
            //@class.NamespaceRef.Add(new SNamespaceRef { ReferenceTo = new SNamespace { NamespaceName = $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Infrastructure" } });

            if (_grpcKProject is KGrpcIntegrationProject)
            {
                @class.Property.Add(new CProperty
                {
                    AccessModifier = CAccessModifier.Public,
                    Type = "ApiHosts",
                    PropertyName = "ApiHosts"
                });

                @class.Property.Add(new CProperty
                {
                    AccessModifier = CAccessModifier.Public,
                    Type = "LimitsSettings",
                    PropertyName = "Limits"
                });

            }
            @class.Property.Add(new CProperty
            {
                AccessModifier = CAccessModifier.Public,
                Type = "int",
                PropertyName = "Port"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Config", FileName = $"{@class.ClassName}.cs" }
            });
        }

        /*
        public void AddLogEvents(CProject project)
        {
            var @class = new CClass ($"LogEvents")
            {
                IsStatic = true,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Infrastructure"
                }
            };
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Logging" }
            });

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Public,
                IsReadonly = true,
                IsStatic = true,
                FieldType = "EventId",
                FieldName = "Generic",
                DefaultValue = @"new EventId(1000, ""generic"")"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Infrastructure", FileName = $"{@class.ClassName}.cs" }
            });
        }*/

        

        //convert Grpc to Model
        protected void AddDomainModelExtensionClass(CProject project, CProject dataProject, CProtoFile protoFile,
            string protoNamespace)
        {
            var modelClasses = BuildModelClasses(_dataStoreKProject, _dataLayerProject, protoFile);

            var builder = new DomainModelExtensionsBuilder();
            foreach (var domainModelClass in modelClasses)
            {
                var extensionClass = builder.BuildExtensionsClass(_grpcKProject, domainModelClass, protoFile, protoNamespace);

                builder.AddToProtoMethods(extensionClass, domainModelClass, protoNamespace);

                builder.AddToModelAsListMethods(extensionClass, domainModelClass, protoFile, protoFile.CSharpNamespace);

                _entityToModelCClassConverter.AddEntityToModelMethods(extensionClass,protoFile, modelClasses, GetEntityClasses(dataProject), protoFile.CSharpNamespace);
                _modelToEntityCClassConverter.AddModelToEntityMethods(extensionClass, domainModelClass, _dataLayerProject, "");
                _modelToProtoCClassConverter.AddModelToProtoMethods(extensionClass,protoFile, domainModelClass, protoNamespace);

                /*
                var enums = BuildEnums(_dataStoreKProject, _dataLayerProject, protoFile);

                foreach (var @enum in  enums)
                {
                    var protoEnum = @enum?.DerivedFrom as CProtoEnum;

                    if (protoEnum == null)
                        continue;

                    var toModelMethod = new CMethod
                    {
                        IsStatic = true,
                        IsExtensionMethod = true,
                        ReturnType = $"{@enum.Namespace.NamespaceName}.{@enum.EnumName}",
                        MethodName = "ToModel"
                    };

                    var parameterType = string.Empty;

                    parameterType = $"{alias}.{protoEnum.EnumName}";//.Rpc.DomainModelNameForOutput}";
                    toModelMethod.Parameter.Add(new CParameter { Type = parameterType, ParameterName = "source" });


                    @class.Method.Add(toModelMethod);
                }
                */

                project.ProjectContent.Add(new CProjectContent
                {
                    Content = extensionClass,
                    BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                    File = new CFile { Folder = $@"Extensions", FileName = $"{extensionClass.ClassName}.cs" }
                });
                
            }
        }



        protected void AddMediatorClass(CProject project, CInterface mediatorInterface)
        {
            var @class = new CClass ($"MediatorExecutor")
            {
                AccessModifier = CAccessModifier.Public,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}"
                }
            };
            @class.Implements.Add(mediatorInterface);
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "MediatR" } });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.DependencyInjection" }
            });


            @class.Constructor.Add(new CConstructor
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = "MediatorExecutor",
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "IServiceScopeFactory", ParameterName = "serviceScopeFactory"}
                },
                CodeSnippet = $"_serviceScopeFactory = serviceScopeFactory;"
            });
            @class.Field.Add(new CField { FieldType = "IServiceScopeFactory", FieldName = "_serviceScopeFactory" });
            @class.Method.Add(new CMethod
            {
                IsAsync = true,
                ReturnType = "Task<T>",
                MethodName = "ExecuteAsync<T>",
                Parameter = new List<CParameter> { new CParameter { Type = "IRequest<T>", ParameterName = "data " } },
                CodeSnippet = @"using (var scope = _serviceScopeFactory.CreateScope())
                        {
                                var svc = scope.ServiceProvider.GetRequiredService<IMediator>();
                        return await svc.Send(data).ConfigureAwait(false);
                    }"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Infrastructure", FileName = $"{@class.ClassName}.cs" }
            });
        }

        protected CInterface AddIMediatorInterface(CProject project)
        {
            var @interface = new CInterface
            {
                InterfaceName = "IMediatorExecutor",
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}"
                }
            };
            @interface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });
            @interface.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "MediatR" } });
            @interface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.DependencyInjection" }
            });

            @interface.Method.Add(new CMethod
            {
                SignatureOnly = true,
                ReturnType = "Task<T>",
                MethodName = "ExecuteAsync<T>",
                Parameter = new List<CParameter> { new CParameter { Type = "IRequest<T>", ParameterName = "data" } }
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @interface,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Infrastructure", FileName = $"{@interface.InterfaceName}.cs" }
            });
            return @interface;
        }

        protected void AddHandlers(CProject project, CProject sqlProject, CInterface dataServiceInterface,
            KGrpcIntegrationProject grpcMIntegrationProject, CProtoFile protoFile)
        {
            foreach (var service in protoFile.ProtoService)
                foreach (var rpc in service.Rpc)
                {
                    var @class = new CClass($"{rpc.RpcName}Handler")
                    {
                        Namespace = new CNamespace
                        {
                            NamespaceName =
                                $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Query"
                        }
                    };
                    var handlerType = rpc.GetReturnType();

                    @class.Implements.Add(new CInterface
                    {
                        InterfaceName = $"IRequestHandler<{rpc.RpcName}Query, {handlerType}>"
                    });
                    
                    @class.NamespaceRef.Add(new CNamespaceRef
                    {
                        ReferenceTo = new CNamespace { NamespaceName = "System" }
                    });

                    @class.NamespaceRef.Add(new CNamespaceRef
                    {
                        ReferenceTo = new CNamespace { NamespaceName = "System.Threading" }
                    });

                    @class.NamespaceRef.Add(new CNamespaceRef
                    {
                        ReferenceTo = new CNamespace { NamespaceName = "System.Linq" }
                    });

                    @class.NamespaceRef.Add(new CNamespaceRef
                    {
                        ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
                    });

                    @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Collections.Generic" } }); //todo: add only if using IList
                    //@class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Linq" } });
                    @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "MediatR" } });
                    //@class.NamespaceRef.Add(new CNamespaceRef {ReferenceTo = new CNamespace {NamespaceName = "Grpc.Core"}});

                    //if (_dataProject == null)
                    /*
                    {
                        @class.NamespaceRef.Add(new CNamespaceRef
                        {
                            ReferenceTo = new CNamespace { NamespaceName = protoFile.CSharpNamespace }
                        });
                    }*/

                    if (_dataLayerProject != null && dataServiceInterface != null)
                    {
                        @class.NamespaceRef.Add(_dataLayerProject.BuildNamespaceRefForType(dataServiceInterface.InterfaceName));

                        var domainModelClasses = BuildModelClasses(_dataStoreKProject, _dataLayerProject, protoFile);
                        if (domainModelClasses.Count > 0)
                        {
                            @class.NamespaceRef.Add( new CNamespaceRef() { ReferenceTo = domainModelClasses.First().Namespace });
                        }
                        //var entityClasses = GetEntityClasses(_dataLayerProject);

                        //if (entityClasses.Count > 0)
                        //    @class.NamespaceRef.Add(_dataLayerProject.BuildNamespaceRefForType(entityClasses.First().ClassName));

                        @class.Constructor.Add(new CConstructor
                        {
                            ConstructorName = @class.ClassName,
                            Parameter = new List<CParameter>
                        {
                            new CParameter {ParameterName = "dataService", Type = dataServiceInterface.InterfaceName}
                        },
                            CodeSnippet = "_dataService = dataService;"
                        });

                        var fieldName = $"_dataService";
                        @class.Field.Add(new CField
                        {
                            AccessModifier = CAccessModifier.Private,
                            IsReadonly = true,
                            FieldName = fieldName,
                            FieldType = dataServiceInterface.InterfaceName
                        }); //todo: fix hard code
                    }

                    var methodReturnType = $"Task<{rpc.GetReturnType()}>";
                    var method = new CMethod
                    {
                        AccessModifier = CAccessModifier.Public,
                        IsAsync = true,
                        ReturnType = methodReturnType,
                        MethodName = "Handle"
                    };
                    method.Parameter.Add(new CParameter { Type = $"{rpc.RpcName}Query", ParameterName = "message" });
                    method.Parameter.Add(new CParameter { Type = $"CancellationToken", ParameterName = "cancellationToken" });

                    /*
                     
                     */
                    if (_dataLayerProject != null)
                    {
                        var codeWriter = GetDataServiceCodeSnippet(rpc, method);
                        method.CodeSnippet = codeWriter.ToString();
                    }
                    else if (grpcMIntegrationProject != null)
                    {
                        var rpcRefs = grpcMIntegrationProject.ProtoRef;
                        foreach (var rpcRef in rpcRefs)
                        {
                            var protoRpc = GetProtoRpc(rpcRef);
                            if (protoRpc != null)
                                @class.NamespaceRef.Add(new CNamespaceRef
                                {
                                    ReferenceTo =
                                        new CNamespace { NamespaceName = protoRpc.ProtoService.ProtoFile.CSharpNamespace }
                                });
                        }
                        var codeWriter = GetIntegrationGrpcServiceCodeSnippet(grpcMIntegrationProject, project, rpc);
                        method.CodeSnippet = codeWriter.ToString();
                    }


                    @class.Method.Add(method);


                    project.ProjectContent.Add(new CProjectContent
                    {
                        Content = @class,
                        BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                        File = new CFile { Folder = $@"Query", FileName = $"{@class.ClassName}.cs" }
                    });
                }
        }


        protected CProtoRpc GetProtoRpc(KProtoRef rpcRef)
        {
            foreach (var project in rpcRef.RefSolution.GeneratedSolution.Project)
            {
                var protoFile = project.GetProtoFile(rpcRef.RefServiceName);
                if (protoFile != null)
                    foreach (var protoService in protoFile.ProtoService)
                    {
                        var rpc = protoService.Rpc.FirstOrDefault(r => r.RpcName == rpcRef.RefRpcName);
                        if (rpc != null)
                            return rpc;
                    }
            }
            return null;
        }

        private CodeWriter GetIntegrationGrpcServiceCodeSnippet(KGrpcIntegrationProject grpcMIntegrationProject,
            CProject project, CProtoRpc rpc)
        {
            var codeWriter = new CodeWriter();
            var rpcRefs = grpcMIntegrationProject.ProtoRef;
            if (rpcRefs.Count == 0)
                return codeWriter;

            codeWriter.WriteLine();

            var index = 1;
            foreach (var rpcRef in rpcRefs)
            {
                var protoRpc = GetProtoRpc(rpcRef);
                if (protoRpc == null)
                    continue;
                var portNumber = _grpcPortService.GeneratePortNumber(_kSolution.SolutionName);
                codeWriter.WriteLine(
                    $@"Channel channel{index} = new Channel(""127.0.0.1:{portNumber}"", ChannelCredentials.Insecure);");

                codeWriter.WriteLine(
                    $@"var client{index} = new {protoRpc.ProtoService.ServiceName}.{
                            protoRpc.ProtoService.ServiceName
                        }Client(channel{index});");
                codeWriter.WriteLine($@"var request{index} = new {protoRpc.Request.MessageName}();");
                codeWriter.WriteLine($"var response{index} = client{index}.{protoRpc.RpcName}(request{index});");
                codeWriter.WriteLine();
                codeWriter.WriteLine();
                index++;
            }
            if (index > 1)
                if (rpc.Response.HasFields)
                    codeWriter.WriteLine($"return new List<{rpc.DomainModelNameForOutput}>();");
                else
                    codeWriter.WriteLine("return true;");
            else
                codeWriter.WriteLine("throw new NotImplementedException();");

            return codeWriter;
        }

        private static CodeWriter GetDataServiceCodeSnippet(CProtoRpc rpc, CMethod method)
        {
            var codeWriter = new CodeWriter();

            var request = rpc.GetInnerMessageOrRequest();
            if (rpc.RequestIsList())
            {

                codeWriter.WriteLine($"foreach (var p in message.{rpc.Request.ProtoField.First().FieldName} )");
                codeWriter.WriteLine("{");
                codeWriter.Indent();
                codeWriter.WriteLine($"var result = await _dataService");
                codeWriter.Indent();
                codeWriter.Write($".{rpc.RpcName}Async(");
                var isFirst = true;
                foreach (var pf in request.ProtoField)
                {
                    if (!isFirst)
                        codeWriter.Write(", ");
                    isFirst = false;
                    codeWriter.Write($"p.{pf.FieldName}");
                }
                codeWriter.WriteLine($")");
                codeWriter.WriteLine(".ConfigureAwait(false);");
                codeWriter.Unindent();
                codeWriter.Unindent();

                codeWriter.WriteLine("}");
            }
            else
            {

                codeWriter.WriteLine($"var result = await _dataService");
                codeWriter.Indent();
                codeWriter.Write($".{rpc.RpcName}Async(");

                var isFirst = true;
                foreach (var pf in request.ProtoField)
                {
                    if (!isFirst)
                        codeWriter.Write(", ");
                    isFirst = false;
                    if (pf.IsEnum)
                    {
                        if (pf.Repeated)
                        {
                            
                        }
                        else
                        {
                            codeWriter.Write("(int)"); //todo: this assumes all Enum's are Int
                        }
                    }
                    codeWriter.Write($"message.{pf.FieldName}");
                    if (pf.IsEnum && pf.Repeated)
                    {
                        codeWriter.Write(".Select(enumVal => (int)enumVal)");
                    }
                }
                codeWriter.WriteLine($")");
                codeWriter.WriteLine(".ConfigureAwait(false);");
                codeWriter.WriteLine();
                codeWriter.Unindent();

                if (string.Compare(method.ReturnType, "Task<bool>", true) == 0)
                {
                    codeWriter.WriteLine("return result;");
                }
                else
                {
                    if (rpc.ResponseIsList())
                    {
                        codeWriter.WriteLine("return result.Select(r=> r.ToModel()).ToList();");
                    }
                    else
                    {
                        codeWriter.WriteLine("throw new NotImplementedException();");
                        codeWriter.WriteLine();

                        codeWriter.WriteLine("//return result.FirstOrDefault()?.ToModel();");
                    }
                }
            }

            return codeWriter;
        }

        protected void AddQuery(CProject project, CProject sqlProject, CProtoFile protoFile)
        {
            foreach (var service in protoFile.ProtoService)
                foreach (var rpc in service.Rpc)
                {
                    var @class = new CClass ($"{rpc.RpcName}Query")
                    {
                        Namespace = new CNamespace
                        {
                            NamespaceName =
                                $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Query"
                        }
                    };

                    var methodReturnType = rpc.GetReturnType();
                    @class.Implements.Add(new CInterface { InterfaceName = $"IRequest<{methodReturnType}>" });

                    @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Collections.Generic" } });
                    @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "MediatR" } });

                    //todo: only add if using WellKnownTypes
                    @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Google.Protobuf.WellKnownTypes" } });

                    var domainModelClasses = BuildModelClasses(_dataStoreKProject, _dataLayerProject, protoFile);
                    if (domainModelClasses.Count > 0)
                    {
                        @class.NamespaceRef.Add(new CNamespaceRef() { ReferenceTo = domainModelClasses.First().Namespace });
                    }

                    foreach (var field in rpc.Request.ProtoField)
                    {
                        var classProperty = new CProperty
                        {
                            AccessModifier = CAccessModifier.Public,
                            PropertyName = field.FieldName
                        };

                        if (field.IsScalar && !field.Repeated)
                            classProperty.Type = SqlMapper.GrpcTypeToClrType(field.FieldType).ToClrTypeName();
                        else if (field.FieldType == GrpcType.__map)
                        {
                            classProperty.Type = "dictionary<string,string>";//todo: fix hard code
                        }
                        else if (field.IsScalar && field.Repeated)
                            classProperty.Type =
                                $"IEnumerable<{SqlMapper.GrpcTypeToClrType(field.FieldType).ToClrTypeName()}>";
                        else if (field.Repeated)
                        {
                            if (field.IsEnum)
                            {
                                classProperty.Type = $"IEnumerable<{field.EnumType}>";
                            }
                            else
                            {
                                classProperty.Type = $"IEnumerable<Model.{field.MessageType}>";
                            }
                        }
                        else if (field.FieldType == GrpcType.__message)
                            classProperty.Type = $"Model.{field.MessageType}";
                        else if (field.FieldType == GrpcType.__enum)
                            classProperty.Type = field.EnumType;


                        @class.Property.Add(classProperty);
                    }

                    project.ProjectContent.Add(new CProjectContent
                    {
                        Content = @class,
                        BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                        File = new CFile { Folder = $@"Query", FileName = $"{@class.ClassName}.cs" }
                    });
                }
        }

        
        protected void AddAppSettingsJson(CProject project, CProtoFile protoFile)
        {
            int portNumber = 999999;
            if (protoFile != null && protoFile.ProtoService.Any())
            {
                portNumber = _grpcPortService.GeneratePortNumber(_kSolution.SolutionName);
            }

            var jsonSnippet = new CodeWriter();
            jsonSnippet.WriteLine("{");
            if (_grpcKProject is KGrpcIntegrationProject)
            {
                jsonSnippet.WriteLine($@"""ApiHosts"": {{}},");
            }

            jsonSnippet.WriteLine($@"
                                    ""Port"": {portNumber},
                                    ""SeriLog"": {{
                                                    ""WriteTo"": [
                                                        {{
                                        ""Name"": ""Console"",
                                        ""theme"": ""AnsiConsoleTheme.Code"",
                                                        ""Args"": {{
                                            ""outputTemplate"": ""{{Timestamp:yyyy-MM-dd HH:mm:ss,fff}} [{{Level}}] {{Message}}{{NewLine}}{{Exception}}""
                                        }}
                                }}
                                    ],
                                    ""MinimumLevel"": ""Debug""
                                    }}");

            //todo move the utility class
            var connectionString = $"Server = localhost; Database ={_dataStoreProject.ProjectShortName}; Trusted_Connection = True;";
            if (_dataLayerKProject.ConnectsToDatabaseType == DataStoreTypes.MySql)
            {
                connectionString = $"Data Source=localhost;Initial Catalog={_dataStoreProject.ProjectShortName};Uid=root;Pwd=my-secret-pw;";
            }
            else if (_dataLayerKProject.ConnectsToDatabaseType == DataStoreTypes.Postgres)
            {
                connectionString = $"Server=localhost;Port=5432; Database={_dataStoreProject.ProjectShortName.ToLower()};User Id=postgres;Password=my-secret-pw;";
            }


            if (_dataStoreProject != null)
            {
                jsonSnippet.WriteLine($@",
                                        ""ConnectionStrings"": {{
                                        ""{_grpcKProject.ProjectName}"": ""{connectionString} ""
                                        }}");

            }

            if (_grpcKProject is KGrpcIntegrationProject)
            {
                jsonSnippet.WriteLine(
                                         $@",
                                        ""Limits"": {{
                                        ""MaxPageSize"": 100
                                        }}
                                    ");
            }

            if (_dataLayerKProject.ConnectsToDatabaseType == DataStoreTypes.Postgres)//TODO: should do for all dbms, only port should be unique
            {
                jsonSnippet.WriteLine(
                                                         $@",
                                        ""CloudformationOutputs"": {{
                                        ""DBEndpoint"": """",
                                        ""Database"": ""{_dataStoreProject.ProjectShortName.ToLower()}"",
                                        ""DBUsername"": """",
                                        ""DBPassword"": """",
                                        ""Port"": ""5432""
                                        }}
                                    ");

            }

            if (true) //TODO:
            {
                jsonSnippet.WriteLine(
                                         $@",
                                        ""Authentication"": {{
                                        ""Enabled"": false,
                                        ""ApiKey"" : """"
                                        }}
                                    ");
            }

            if (true) //TODO:
            {
                jsonSnippet.WriteLine(
                                         $@",
                                        ""Tracing"": {{
                                        ""Enabled"": false,
                                        ""TracingAddress"" : ""localhost:8126"",
                                        ""ServiceName"" : ""todo""
                                        }}
                                    ");
            }
            jsonSnippet.WriteLine("}");
            dynamic parsedJson = JsonConvert.DeserializeObject(jsonSnippet.ToString());
            var formattedJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

            var appSettingsFile = new CText { Text = formattedJson };

            project.ProjectContent.Add(new CProjectContent
            {
                Content = appSettingsFile,
                BuildAction = CBuildAction.None,
                CopyToOutputDirectory = true,
                File = new CFile { Folder = $@"", FileName = $"appsettings.json" }
            });
        }

        protected void AddDockerFile(CProject project)
        {
            var dockerFile = _cProjectToDockerFileConverter.Convert(project);
            project.ProjectContent.Add(new CProjectContent
            {
                Content = dockerFile,
                BuildAction = CBuildAction.None,
                CopyToOutputDirectory = true,
                CopyToPublishDirectory = true,
                File = new CFile { Folder = $@"", FileName = $"Dockerfile", Encoding = Encoding.ASCII }
            });
        }

        protected void AddStartupAppServices(CProject project, CClass dbProviderClass, CInterface dbProviderInterface)
        {
            var @class = new CClass("ServiceCollectionAppServicesExtensions")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Startup"
                },
                IsStatic = true
            };
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
                ReferenceTo = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}.{_grpcKProject.ProjectSuffix}.Config"
                }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}.{_grpcKProject.ProjectSuffix}.Infrastructure"
                }
            });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Options" }
            });

            if (dbProviderInterface != null)
                @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = dbProviderInterface.Namespace });

            
            
            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Startup", FileName = $"Startup.AppServices.cs" }
            });
        }
        void AddHealthCheckQuery(CProject project)
        {
            var @class = new CClass("HealthCheckQuery")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                     $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Query"
                },
                Implements = new List<CInterface> { new CInterface { InterfaceName = "IRequest<bool>" } }
            };

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "MediatR" } });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Query", FileName = $"{@class.ClassName}.cs" }
            });
        }

        void AddHealthCheckHandler(CProject project)
        {
            var @class = new CClass("HealthCheckHandler")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                     $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Query"
                },
                Implements = new List<CInterface> { new CInterface { InterfaceName = "IRequestHandler<HealthCheckQuery, bool>" } }
            };

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Threading" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "MediatR" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}.Data.DataServices" } });

            @class.Field.Add(new CField { AccessModifier = CAccessModifier.Private, IsReadonly = true, FieldType = "IDataHealthCheckService", FieldName = "_dataHealthCheckService" });

            @class.Constructor.Add(new CConstructor
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = "HealthCheckHandler",
                Parameter = new List<CParameter> { new CParameter { Type = "IDataHealthCheckService", ParameterName = "dataHealthCheckService" } },
                CodeSnippet = "_dataHealthCheckService = dataHealthCheckService;"
            });

            @class.Method.Add(new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "Task<bool>",
                MethodName = "Handle",
                Parameter = new List<CParameter>
                {
                    new CParameter { Type = "HealthCheckQuery", ParameterName = "request" },
                    new CParameter { Type = "CancellationToken", ParameterName = "cancellationToken" }

                },
                CodeSnippet = "return _dataHealthCheckService.Check();"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Query", FileName = $"{@class.ClassName}.cs" }
            });
        }

        void AddHealthServiceImpl(CProject project)
        {
            var @class = new CClass("HealthServiceImpl")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Implementation"
                },
                InheritsFrom = new CClass ( "Health.HealthBase" )
            };

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Health.V1" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Logging" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{project.ProjectName}.Query" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Company.GrpcCommon.Infrastructure" } });

            @class.Field.Add(new CField { AccessModifier = CAccessModifier.Private, IsReadonly = true, FieldType = "IMediatorExecutor", FieldName = "_executor" });
            @class.Field.Add(new CField { AccessModifier = CAccessModifier.Private, IsReadonly = true, FieldType = "ILogger<HealthServiceImpl>", FieldName = "_logger" });

            @class.Constructor.Add(new CConstructor
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = "HealthServiceImpl",
                Parameter = new List<CParameter> { new CParameter { Type = "IMediatorExecutor", ParameterName = "executor" }, new CParameter { Type = "ILogger<HealthServiceImpl>", ParameterName = "logger" } },
                CodeSnippet =
              @"_executor = executor;
                _logger = logger;"
            });

            @class.Method.Add(new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                IsOverride = true,
                IsAsync = true,
                ReturnType = "Task<HealthCheckResponse>",
                MethodName = "Check",
                Parameter = new List<CParameter> { new CParameter { Type = "HealthCheckRequest", ParameterName = "request" }, new CParameter { Type = "ServerCallContext", ParameterName = "context" } },
                CodeSnippet =
                @"var status = HealthCheckResponse.Types.ServingStatus.Serving;

                try
                {
                    if (!await _executor.ExecuteAsync(new HealthCheckQuery()))
                    {
                        status = HealthCheckResponse.Types.ServingStatus.NotServing;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(""Error running health check: {0}"", e);
                    status = HealthCheckResponse.Types.ServingStatus.NotServing;
                }

                return new HealthCheckResponse
                {
                    Status = status
                };"
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Implementation", FileName = $"{@class.ClassName}.cs" }
            });
        }
       

        protected void AddLoggerClass(CProject project)
        {
            /*
            var @class = new CClass
            {
                Namespace = new SNamespace { NamespaceName = $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}" },
                ClassName = "GrpcLogger",
              
            };
            @class.Implements.Add(new SInterface { InterfaceName = "Grpc.Core.Logging.ILogger" });

            @class.Field.Add(new SField {AccessModifier = SAccessModifier.Private, FieldType= "ILoggerFactory", FieldName = "_loggerFactory" });

            @class.Field.Add(new SField { AccessModifier = SAccessModifier.Private, FieldType = "ILogger", FieldName = "_logger" });

            @class.NamespaceRef.Add(new SNamespaceRef { ReferenceTo = new SNamespace { NamespaceName = "System" } });
            @class.NamespaceRef.Add(new SNamespaceRef { ReferenceTo = new SNamespace { NamespaceName = "Microsoft.Extensions.Logging" } });
            */
            //todo: build real CClass,
            //for now just copy in the text


            var text = new CText();
            text.Text = ReadResourceFile("GrpcLogger.cs");
            text.Text = text.Text.Replace("NamespacePlaceholder",
                $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}");
            project.ProjectContent.Add(new CProjectContent
            {
                Content = text,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Logger", FileName = $"GrpcLogger.cs" }
            });
        }

        protected void AddAppClass(CProject project)
        {
            var @class = new CClass("App")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}"
                },
                Implements = new List<CInterface>
                {
                    new CInterface() { InterfaceName = "IHostedService"}
                }
            };

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Threading" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core" } });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Configuration" }
            });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Hosting" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Logging" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = $"{@class.Namespace.NamespaceName}.Startup" }
            });

            
            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "IServiceProvider",
                FieldName = "_serviceProvider"
            });

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "ILoggerFactory",
                FieldName = "_loggerFactory"
            });

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "IConfiguration",
                FieldName = "_configuration"
            });

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "IHostEnvironment",
                FieldName = "_hostingEnvironment"
            });

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                FieldType = "Server",
                FieldName = "_server"
            });
            
            
            //private ILogger<App> Logger { get; set; }
            @class.Property.Add(new CProperty
            {
                AccessModifier = CAccessModifier.Private,
                Type = "ILogger<App>",
                PropertyName = "Logger"
            });

            /*
            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsConst = true,
                FieldType = "string",
                FieldName = "EnvPrefix",
                DefaultValue = $@"""{ _grpcKProject.CompanyName.ToUpper().Replace(".", "_") }_{
                   _grpcKProject.ProjectName.ToUpper().Replace(".", "_")
               }_SVC_"""
            });
            */
            @class.Constructor.Add(new CConstructor()
            {
                ConstructorName = "App",
                Parameter = new List<CParameter>()
                {
                    new CParameter() {Type = "IServiceProvider", ParameterName = "serviceProvider"},
                    new CParameter() {Type = "ILoggerFactory", ParameterName = "loggerFactory"},
                    new CParameter() {Type = "IConfiguration", ParameterName = "configuration"},
                    new CParameter() {Type = "IHostEnvironment", ParameterName = "hostingEnvironment"}
                },
                CodeSnippet = 
                  @" _serviceProvider = serviceProvider;
                    _loggerFactory = loggerFactory;
                    _configuration = configuration;
                    _hostingEnvironment = hostingEnvironment;
                    Logger = loggerFactory.CreateLogger<App>();"
            });
            @class.Method.Add(new CMethod
            {
                IsAsync = false,
                ReturnType = "Task",
                MethodName = "StartAsync",
                Parameter = new List<CParameter>()
                {
                    new CParameter() {Type = "CancellationToken", ParameterName = "cancellationToken"}
                },
                CodeSnippet =
                  @"Logger.LogInformation(""Starting application"");
                    Logger.LogInformation(""Environment: {0}"", _hostingEnvironment.EnvironmentName);

                    _server = _serviceProvider.AddGrpcServices(_loggerFactory, _configuration);
                    _server.Start();

                    return Task.CompletedTask;"
            });

                /*
                @class.Method.Add(new CMethod
                {
                    IsAsync = true,
                    ReturnType = "Task",
                    MethodName = "StartAsync",
                    Parameter = new List<CParameter>()
                    {
                        new CParameter() { Type = "CancellationToken", ParameterName = "cancellationToken"}
                    },
                    CodeSnippet =
            $@" Configuration = new ConfigurationBuilder()
                    // json file is the first config layer
                    .AddJsonFile(""appsettings.json"", optional: false)
                    .AddJsonFile($""appsettings.{{EnvironmentName}}.json"", optional: true)
                    // override it with environment variables with specified prefix
                    .AddEnvironmentVariables(prefix: EnvPrefix)
                    // override it with config-center-service variables
                    .AddConfigCenter(""{_kSolution.SolutionName.ToLower()}-service"", EnvPrefix)
                    .Build();

                _serviceProvider = new ServiceCollection()
                    .AddSingleton(Configuration)
                    .AddLogging()
                    .AddOptions()
                    .Configure<CloudformationOutputs>(Configuration.GetSection(""CloudformationOutputs""))
                    .Configure<AuthenticationSettings>(Configuration.GetSection(""Authentication""))
                    .AddAppServices(Configuration)
                    .ConfigureContainer();

                     var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
                     ConfigureLogger(loggerFactory);

                _server = _serviceProvider.AddGrpcServices(loggerFactory, Configuration.GetValue<int>(""Port""));

                await StartServer(_serviceProvider.GetService<IDataHealthCheckService>()).ConfigureAwait(false);"
                });
                */

                @class.Method.Add(new CMethod
            {
                IsAsync = true,
                ReturnType = "Task",
                MethodName = "StopAsync",
                Parameter = new List<CParameter>()
                {
                    new CParameter() {Type = "CancellationToken", ParameterName = "cancellationToken"}
                },
                CodeSnippet =
                      @"Logger.LogInformation(""Begin stopping gRPC server: server.ShutdownAsync(), wait for requests to complete"");
                        await _server.ShutdownAsync();
                        Logger.LogInformation(""End stopping gRPC server"");"
            });
            /*
            @class.Method.Add(new CMethod
            {
                ReturnType = "void",
                MethodName = "ConfigureLogger",
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "ILoggerFactory", ParameterName = "loggerFactory"}
                },
                CodeSnippet =
                @" Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(Configuration)
                .CreateLogger();

            loggerFactory.AddSerilog();

            Logger = loggerFactory.CreateLogger<App>();
            Logger.LogInformation(""Starting application"");
            Logger.LogInformation(""Environment: {0}"", EnvironmentName); 
            if (Configuration.Providers.Any(p => p.GetType() == typeof(ConfigCenterConfigurationProvider)))
            {
                Logger.LogInformation(""Using config center configuration provider"");
            }"
            });
            */

            /*
            @class.Method.Add(new CMethod
            {
                IsAsync = true,
                ReturnType = "Task",
                AccessModifier = CAccessModifier.Private,
                MethodName = "StartServer",
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "IDataHealthCheckService", ParameterName = "dataHealthCheckService"}
                },
                CodeSnippet =
              @"try
                {
                    if (await dataHealthCheckService.Check().ConfigureAwait(false))
                    {
                        _server.Start();
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(""Failed starting application: {0}"", e);
                    Environment.Exit(1);
                }"
            });
            */

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"", FileName = $"{@class.ClassName}.cs" }
            });
        }

        protected void AddStartupGrpcClass(CProject project, IList<KProtoFile> mProtoFiles)
        {
            var @class = new CClass("ServiceProviderExtensions")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Startup"
                },

                AccessModifier = CAccessModifier.Public,
                IsStatic = true
            };
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Collections.Generic" } });

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core.Interceptors" } });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core.Logging" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Grpc.Health.V1" }
            });

            @class.NamespaceRef.Add(
                new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Reflection" } });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Grpc.Reflection.V1Alpha" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.DependencyInjection" }
            });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Logging" }
            });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Configuration" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Config" }
            });

            /*
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Interceptors" }
            });
            */

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { Alias = "HealthServiceImpl", NamespaceName = $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}.Implementation.HealthServiceImpl" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.GrpcCommon" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.GrpcCommon.Logging" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.GrpcCommon.Interceptors" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.GrpcCommon.Tracing.Interceptors" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { Alias = "LogLevel", NamespaceName = "Grpc.Core.Logging.LogLevel" }
            });

            // private const int DefaultPort = 50060;
            int portNumber = 999999;

            if (mProtoFiles.Count > 0)
            {
                portNumber =
                  _grpcPortService.GeneratePortNumber(_kSolution.SolutionName);
            }

            @class.Field.Add(new CField
            {
                FieldName = "DefaultPort",
                IsConst = true,
                FieldType = "int",
                AccessModifier = CAccessModifier.Private,
                DefaultValue = $"{portNumber}"
            });
            // private static int GetPort(int port) => port > 0 ? port : DefaultPort;


            var methodAddGrpcServices = new CMethod
            {
                IsStatic = true,
                IsExtensionMethod = true,
                ReturnType = "Server",
                MethodName = "AddGrpcServices",
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "IServiceProvider", ParameterName = "provider"},
                    new CParameter {Type = "ILoggerFactory", ParameterName = "loggerFactory"},
                    new CParameter {Type = "IConfiguration", ParameterName = "configuration"}
                }
            };
            var methodSnippet = new CodeWriter();
            methodSnippet.WriteLine($@"  
            var servicePort = GetPort(configuration);
            GrpcEnvironment.SetLogger(new LogLevelFilterLogger(new GrpcLogger(loggerFactory), LogLevel.Debug));
            GrpcEnvironment.Logger.Debug($@""Starting {_grpcKProject.ProjectName} services on port {{servicePort}}"");

            var logger = loggerFactory.CreateLogger<LoggingInterceptor>();

            var builder = new GrpcServerBuilder()
                .ConfigureServerOptions(options =>
                {{
                    // disconnect IDLE connection after 5 minutes
                    options.MaxConnectionIdleMs = 300000;
                }})
                .AddInsecurePort(servicePort);
            
            var services = new List<ServerServiceDefinition>
            {{");
            methodSnippet.Indent();
            
            var first = true;
            foreach (var mProtoFile in mProtoFiles)
            {
                @class.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = mProtoFile.GeneratedProtoFile.CSharpNamespace }
                });
               

                foreach (var protoService in mProtoFile.GeneratedProtoFile.ProtoService)
                {
                    if (!first)
                        methodSnippet.WriteLine(",");

                    methodSnippet.WriteLine(
                        $@"{protoService.ServiceName}.BindService(provider.GetRequiredService<{
                                protoService.ServiceName
                            }Impl>())");
                    methodSnippet.Indent();
                    methodSnippet.WriteLine(".Intercept(new LoggingInterceptor(logger, Microsoft.Extensions.Logging.LogLevel.Information)),");
                    methodSnippet.Unindent();
                    methodSnippet.WriteLine($@"Health.BindService(provider.GetRequiredService<HealthServiceImpl>())");
                    methodSnippet.Indent();
                    methodSnippet.WriteLine($@".Intercept(new LoggingInterceptor(logger, Microsoft.Extensions.Logging.LogLevel.Debug)),");
                    methodSnippet.Unindent();
                    first = false;
                }
            }
            methodSnippet.Unindent();
            methodSnippet.WriteLine($@"}};");
            


            methodSnippet.WriteLine();
            methodSnippet.WriteLine("AddTracing(provider, services, configuration);");
            methodSnippet.WriteLine();
            methodSnippet.WriteLine("builder.AddServices(services);");
            methodSnippet.WriteLine();
            methodSnippet.WriteLine(GetAddServiceSnippet());
            methodSnippet.WriteLine();
            methodSnippet.WriteLine($@"return builder.Build();");
            methodAddGrpcServices.CodeSnippet = methodSnippet.ToString();
            @class.Method.Add(methodAddGrpcServices);

            var methodGetDefaultPort = new CMethod
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                ReturnType = "int",
                MethodName = "GetPort",
                UseExpressionDefinition = false
            };
            methodGetDefaultPort.Parameter.Add(new CParameter { ParameterName = "configuration", Type = "IConfiguration" });
            methodGetDefaultPort.CodeSnippet = 
                  @"var port = configuration.GetValue<int>(""Port"");
                    return port > 0 ? port : DefaultPort;";


            @class.Method.Add(methodGetDefaultPort);

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                ReturnType = "void",
                MethodName = "AddTracing",
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = "IServiceProvider", ParameterName = "provider"},
                    new CParameter() { Type = "IList<ServerServiceDefinition>", ParameterName = "services"},
                    new CParameter() { Type = "IConfiguration", ParameterName = "configuration"}
                },
                CodeSnippet = GetTracingCodeSnippet()
                
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Startup", FileName = $"Startup.GrpcServers.cs" }
            });
        }

        private string GetAddServiceSnippet()
        {
            return
                 $@"builder.AddService(ServerReflection.BindService(new ReflectionServiceImpl(
                                {_grpcKProject.ProjectNameAsClassNameFriendly}Service.Descriptor,
                                Health.Descriptor)));";
        }

        private string GetTracingCodeSnippet()
        {
            return 
          @"var tracingOptions = new TracingOptions(configuration);
            if (!tracingOptions.Enabled)
            {
                return;
            }

            var serverTracingInterceptor = provider.GetRequiredService<ServerTracingInterceptor>();

            for (var i = 0; i < services.Count; i++)
            {
                services[i] = services[i].Intercept(serverTracingInterceptor);
            }";
        }

      
        protected void AddProgramClass(CProject project, CClass containerClass)
        {
            var @class = new CClass("Program")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}"
                }
            };

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Configuration" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.DependencyInjection" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.Extensions.Hosting" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Serilog" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Lamar" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Lamar.Microsoft.DependencyInjection" }
            });



            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.Configuration.ConfigCenter" }
            });


            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = $"{@class.Namespace.NamespaceName}.Infrastructure" }
            });

            //@class.NamespaceRef.Add(new CNamespaceRef() { ReferenceTo = containerClass.Namespace });

            @class.Field.Add(new CField
            {
                Comment = "number of minutes to wait before forced shutdown",
                AccessModifier = CAccessModifier.Private, IsConst = true, FieldType = "int", FieldName = "ShutdownWaitTime", DefaultValue = "1"

            });

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsConst = true,
                FieldType = "string",
                FieldName = "EnvPrefix",
                DefaultValue = $@"""{ _grpcKProject.CompanyName.ToUpper().Replace(".", "_") }_{
                        _grpcKProject.ProjectName.ToUpper().Replace(".", "_")
                    }_SVC_"""
            });

            @class.Method.Add(new CMethod
            {
                IsAsync = true,
                ReturnType = "Task",
                IsStatic = true,
                MethodName = "Main",
                Parameter = new List<CParameter> { new CParameter { Type = "string[]", ParameterName = "args" } },
                CodeSnippet =
                     $@"
                        var hostBuilder = new HostBuilder()
                            .UseLamar()
                            .ConfigureAppConfiguration((context, builder) =>
                            {{
                                builder.AddJsonFile(""appsettings.json"", optional: false)
                                    .AddEnvironmentVariables(prefix: EnvPrefix)
                                    // override it with config-center-service variables
                                    .AddConfigCenter(""{_kSolution.SolutionName.ToLower()}-service"", EnvPrefix);
                            }})
                            .UseSerilog((hostingContext, loggerConfiguration) =>
                            {{
                                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                            }})
                            .ConfigureContainer((HostBuilderContext context, ServiceRegistry registry) =>
                            {{
                                registry.Configure(context.Configuration);
                            }})
                            .ConfigureServices((context, services) =>
                            {{
                                services.AddHostedService<App>();
                                services.Configure<HostOptions>(opts =>
                                {{
                                    opts.ShutdownTimeout = TimeSpan.FromMinutes(ShutdownWaitTime);
                                }});
                            }});                        

                        await hostBuilder.RunConsoleAsync();
                        "
            });

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"", FileName = $"{@class.ClassName}.cs" }
            });
        }

        protected void AddNugetRefs(CProject project)
        {
            project.NuGetReference.Add(new CNuGet { NuGetName = "MediatR", Version = "4.1.0" });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Google.Protobuf.Tools", Version = _configuration.GetValue<string>("Google_Protobuf_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Google.Protobuf", Version = _configuration.GetValue<string>("Google_Protobuf_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc", Version = _configuration.GetValue<string>("Grpc_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc.Tools", Version = _configuration.GetValue<string>("Grpc_Tools_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc.Core", Version = _configuration.GetValue<string>("Grpc_Core_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc.Reflection", Version = _configuration.GetValue<string>("Grpc_Reflection_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc.HealthCheck", Version = _configuration.GetValue<string>("Grpc_HealthCheck_NugetVersion") });
            project.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.DependencyInjection",
                Version = "2.1.1"
            });

            project.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Lamar.Microsoft.DependencyInjection",
                Version = "4.0.2"
            });

            project.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Hosting",
                Version = "2.1.1"
            });
            project.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Configuration",
                Version = "2.1.1"
            });
            project.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Configuration.Json",
                Version = "2.1.1"
            });
            project.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Configuration.EnvironmentVariables",
                Version = "2.1.1"
            });
            project.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Options.ConfigurationExtensions",
                Version = "2.1.1"
            });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Microsoft.Extensions.Logging", Version = "2.1.1" });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Serilog.Extensions.Hosting", Version = "3.0.0" });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Serilog.Formatting.Compact", Version = "1.1.0" });

            project.NuGetReference.Add(new CNuGet { NuGetName = "Serilog.Settings.Configuration", Version = "2.6.1" });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Serilog.Sinks.Console", Version = "3.1.1" });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Company.GrpcCommon", Version = _configuration.GetValue<string>("Company_GrpcCommon_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Company.GrpcCommon.Tracing", Version = "2.0.7" });

            project.NuGetReference.Add(new CNuGet { NuGetName = "Company.Configuration.ConfigCenter", Version = _configuration.GetValue<string>("Company_Configuration_ConfigCenter_NugetVersion") });

            //project.NuGetReference.Add(new CNuGet { NuGetName = "System.Runtime.Loader", Version = "4.3.0" });
            //project.NuGetReference.Add(new CNuGet { NuGetName = "Mono.Posix.NETStandard", Version = "1.0.0" });

        }

        protected void AddProtoBatchFile(CProject project)
        {
            var generateProtoBatchFile = ReadResourceFile("Grpc.regen-grpc.cmd");
            project.ProjectContent.Add(new CProjectContent
            {
                Content = new CBatchFile { BatchFileContent = generateProtoBatchFile, ExecutePostKickstart = true },
                BuildAction = CBuildAction.None,
                File = new CFile { Folder = $@"", FileName = $"regen-grpc.cmd" }
            });

            var generateProtoBatchFileScript = ReadResourceFile("Grpc.regen-grpc.sh");
            project.ProjectContent.Add(new CProjectContent
            {
                Content = new CText { Text = generateProtoBatchFileScript },
                BuildAction = CBuildAction.None,
                File = new CFile { Folder = $@"", FileName = $"regen-grpc.sh" }
            });
        }

        protected void AddProtoRefBatchFile(CProject project)
        {
            var generateProtoBatchFile = ReadResourceFile("regen-grpc.cmd");

            project.ProjectContent.Add(new CProjectContent
            {
                Content = new CBatchFile { BatchFileContent = generateProtoBatchFile },
                BuildAction = CBuildAction.None,
                File = new CFile { Folder = $@"Proto\ProtoRef", FileName = $"regen-grpc.cmd" }
            });
        }

        private IList<KProtoFile> BuildProtoFile(KDataStoreProject databaseProject, KDataLayerProject dataLayerProject,
            KGrpcProject grpcKProject)
        {
            //converter.NamespaceName = protoNamespace;
            //converter.ServiceName = $@"{_grpcKProject.ProjectName}{_grpcKProject.ProjectSuffix}";
            //converter.BulkStoreRpcName = _grpcKProject.BulkStoreRpcName; //  $"BulkStore{_grpcKProject.ProjectName}";

            var protoFiles = _kDataLayerProjectToKProtoFileConverter.Convert(databaseProject, dataLayerProject, _grpcKProject);
            //protoFile.OwnedByProject = project;
            return protoFiles;
        }

        /*
        private SProtoFile BuildProtoFile(SProject project, SProject sqlProject, string protoNamespace)
        {
            var converter = new SStoredProcedureToSProtoFileConverter();
            converter.NamespaceName = protoNamespace;
            converter.ServiceName = $@"{_grpcKProject.ProjectName}{_grpcKProject.ProjectSuffix}";
            //converter.BulkStoreRpcName = _grpcKProject.BulkStoreRpcName; //  $"BulkStore{_grpcKProject.ProjectName}";

            SProtoFile protoFile = converter.Convert(GetStoredProcedures(sqlProject), _grpcKProject.KickstartCRUD, _grpcKProject.KickstartBulkStore);
            //protoFile.OwnedByProject = project;
            return protoFile;
        }*/
        protected void AddProtoFile(CProject project, CProtoFile protoFile)
        {
            /*
            var protoJson = JsonConvert.SerializeObject(protoFile, Formatting.Indented);
            project.ProjectContent.Add(new SProjectContent()
            {
                Content = new SText { Text = protoJson },
                BuildAction = SBuildAction.None,
                File = new SFile() { Folder = $@"Proto", FileName = $"{protoFile.ProtoFileName}.json" }
            });
            */
            project.ProjectContent.Add(new CProjectContent
            {
                Content = protoFile,
                BuildAction = CBuildAction.None,
                File = new CFile { Folder = $@"Proto", FileName = $"{protoFile.ProtoFileName}.proto" }
            });
        }

        private string ReadResourceFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Kickstart.Core.NetStandard.Boilerplate.{fileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private List<CStoredProcedure> GetStoredProcedures(CProject sqlProject)
        {
            var storedProcedures = new List<CStoredProcedure>();
            foreach (var pc in sqlProject.ProjectContent)
                if (pc.Content is CStoredProcedure)
                    storedProcedures.Add(pc.Content as CStoredProcedure);

            return storedProcedures;
        }

        private List<CClass> GetEntityClasses(CProject dataProject)
        {
            var classes = new List<CClass>();
            if (dataProject != null)
                foreach (var pc in dataProject.ProjectContent)
                {
                    if (pc.File.Folder != "Entities")
                        continue;

                    if (pc.Content is CClass)
                        classes.Add(pc.Content as CClass);
                }

            return classes;
        }
    }
}