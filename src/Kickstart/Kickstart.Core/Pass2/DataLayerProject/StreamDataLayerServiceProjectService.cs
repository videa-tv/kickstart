using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject
{
    public class StreamDataLayerServiceProjectService : DataLayerServiceProjectServiceBase, IDataLayerServiceProjectService
    {
       
        public string ConnectionString { get; set; }
        public DataStoreTypes ConnectsToDatabaseType { get; set; }

        public CProject BuildProject(KDataStoreProject databaseKProject, KDataLayerProject dataLayerKProject, IEnumerable<CStoredProcedure> storedProceduresIn, IEnumerable<KTable> tables, IEnumerable<KTableType> tableTypes, IEnumerable<CView> views)
        {
            _dataStoreKProject = databaseKProject;   
            _dataLayerKProject = dataLayerKProject;
            var dataProject = new CProject
            {
                ProjectName = $"{dataLayerKProject.ProjectFullName}.{dataLayerKProject.ConnectsToDatabaseType}",
                ProjectShortName = dataLayerKProject.ProjectShortName,
                ProjectFolder = $"{dataLayerKProject.ProjectFolder}.{dataLayerKProject.ConnectsToDatabaseType}",
                ProjectType = CProjectType.CsProj,
                ProjectIs = CProjectIs.DataAccess
            };

            dataProject.TemplateProjectPath =
                @"templates\NetStandard20ClassLibrary.csproj";

            var dbProviderInterface = BuildDbProviderInterface();
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Interface = dbProviderInterface,
                File = new CFile { Folder = "Providers", FileName = $"{dbProviderInterface.InterfaceName}.cs" }
            });


            var dbProvider = BuildDbProviderClass(dbProviderInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = dbProvider,
                File = new CFile { Folder = "Providers", FileName = $"{dbProvider.ClassName}.cs" }
            });

            var dataHealthCheckInterface = BuildDataHealthCheckInterface();
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Interface = dataHealthCheckInterface,
                File = new CFile { Folder = "DataServices", FileName = $"{dataHealthCheckInterface.InterfaceName}.cs" }
            });

            var baseDataService = BuildBaseDataServiceClass(dbProviderInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = baseDataService,
                File = new CFile { Folder = "DataServices\\Base", FileName = $"{baseDataService.ClassName}.cs" }
            });


            var dataHealthCheckClass = BuildDataHealthCheckClass(dataHealthCheckInterface, baseDataService, dbProviderInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = dataHealthCheckClass,
                File = new CFile { Folder = "DataServices", FileName = $"{dataHealthCheckClass.ClassName}.cs" }
            });

            var storedProcedures = databaseKProject.StoredProcedure.Select(s => s.GeneratedStoredProcedure).ToList();

            var dtoClassesAll = BuildEntityClasses(storedProcedures, tables, tableTypes, views, dataProject);

            var dataServiceInterface = BuildDataServiceInterface(dtoClassesAll);
            dataLayerKProject.DataServiceInterface.Add(dataServiceInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Interface = dataServiceInterface,
                File = new CFile { Folder = "DataServices", FileName = $"{dataServiceInterface.InterfaceName}.cs" }
            });

            var dataService = BuildDataServiceClass(dataServiceInterface, dbProviderInterface, dtoClassesAll);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = dataService,
                File = new CFile { Folder = "DataServices", FileName = $"{dataService.ClassName}.cs" }
            });

            if (_dataLayerKProject.ConnectsToDatabaseType == DataStoreTypes.Kinesis)
            {
                dataProject.NuGetReference.Add(new CNuGet { NuGetName = "AWSSDK.S3", Version = "3.3.18.4" });
            }
            else
            {
                throw new NotImplementedException();
            }

            return dataProject;
        }

        private CInterface BuildDbProviderInterface()
        {
            var dbProviderInterface = new CInterface();

            dbProviderInterface.InterfaceName = $"I{_dataLayerKProject.ProjectName}DbProvider";

            dbProviderInterface.Namespace = new CNamespace
            {
                NamespaceName =
                   $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Providers"
            };
            return dbProviderInterface;
        }

        private CClass BuildDbProviderClass(CInterface dbProviderInterface)
        {
            var dbProvider = new CClass($"{_dataLayerKProject.ProjectName}DbProvider");
            dbProvider.Implements.Add(dbProviderInterface);
            dbProvider.Namespace = new CNamespace
            {
                NamespaceName =
                   $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Providers"
            };
            return dbProvider;
        }

       
        private CClass BuildDataHealthCheckClass(CInterface dataHealthCheckInterface, CClass baseDataService, CInterface dbProviderInterface)
        {
            var @class = new CClass("DataHealthCheckService")
            {
                AccessModifier = CAccessModifier.Public,
                Implements = new List<CInterface>() { dataHealthCheckInterface },
                InheritsFrom = baseDataService
            };

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System" }
            });


            
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });
            /*
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Dapper" }
            });
            */

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = baseDataService.Namespace });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = dbProviderInterface.Namespace });


            @class.Namespace = new CNamespace { NamespaceName = dataHealthCheckInterface.Namespace.NamespaceName };

            var constructor = new CConstructor
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = "DataHealthCheckService",
                Parameter = new List<CParameter>
            {
                new CParameter { Type= dbProviderInterface.InterfaceName,  ParameterName = "dbProvider", PassToBaseClass = true },

            },
                CodeSnippet = "//Empty Constructor"
            };

            @class.Constructor.Add(constructor);

            var methodCheck = new CMethod { ReturnType = "Task<bool>", MethodName = "Check", IsAsync = true, AccessModifier = CAccessModifier.Public };
            /*
            methodCheck.CodeSnippet =
                @"using var connection = DbProvider.GetConnection();
           
                return await connection.QueryFirstAsync<bool>(""select 1"").ConfigureAwait(false);
            ";
            */
            methodCheck.CodeSnippet = "throw new NotImplementedException();";
            @class.Method.Add(methodCheck);
            
            return @class;
        }
        
        private CClass BuildDataServiceClass(CInterface dataServiceInterface, CInterface dataProviderInterface, List<CClass> dtoClasses)
        {
            var dataService = new CClass($"{_dataLayerKProject.ProjectName}{_dataLayerKProject.ProjectSuffix}Service")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.{_dataLayerKProject.ProjectSuffix}.DataServices"
                },
                IsAbstract = false,
                InheritsFrom = new CClass("BaseDataService"),
                Implements = new List<CInterface> { new CInterface { InterfaceName = dataServiceInterface.InterfaceName } }
            };
            dataService.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System" }
            });
            dataService.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Collections.Generic" }
            });
            dataService.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Data" }
            });
            dataService.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Linq" }
            });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Reflection" } });

            dataService.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });

            //dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Dapper" } });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.DataServices.Base" } });

            //dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Extensions" } });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities" } });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = dataProviderInterface.Namespace });


            /*
            if (ConnectsToDatabaseType == DatabaseTypes.Postgres)
            {
                dataService.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = "Npgsql" }
                });
            }*/

            //dataService.Field.Add(new SField() { FieldName = "DefaultPageNumber", FieldType = "int", IsReadonly = false });
            //dataService.Field.Add(new SField() { FieldName = "DefaultPageSize", FieldType = "int", IsReadonly = false });

            var constructor = new CConstructor
            {
                IsStatic = false,
                ConstructorName = dataService.ClassName,
                CodeSnippet = "//empty constructor"
            };

            constructor.Parameter.Add(new CParameter
            {
                PassToBaseClass = true,
                ParameterName = "dbProvider",
                Type = $"I{_dataLayerKProject.ProjectName}DbProvider"
            });

            dataService.Constructor.Add(constructor);

            var methods = GetDataServiceMethods();

            foreach (var m in methods)
            {
                m.CodeSnippet = "throw new NotImplementedException();";
            }

            dataService.Method.AddRange(methods);


            //todo: clean this up
           // if (dtoClasses.Count > 0)
           //     dataService.NamespaceRef.Add(_dataProject.BuildNamespaceRefForType(dtoClasses.First().ClassName));
            return dataService;
        }
        private CClass BuildBaseDataServiceClass(CInterface dbProviderInterface)
        {
            var dataService = new CClass($"BaseDataService") { IsAbstract = true };

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = dbProviderInterface.Namespace });

            dataService.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.DataServices.Base"
            };
            dataService.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Protected,
                FieldName = "DbProvider",
                FieldType = $"I{_dataLayerKProject.ProjectName}DbProvider",
                IsReadonly = true
            });

            var staticConstructor = new CConstructor
            {
                IsStatic = true,
                AccessModifier = CAccessModifier.Private,
                ConstructorName = "BaseDataService" 
            };
            dataService.Constructor.Add(staticConstructor);

            var constructor = new CConstructor
            {
                ConstructorName = dataService.ClassName,
                AccessModifier = CAccessModifier.Protected
            };
            constructor.Parameter.Add(new CParameter
            {
                ParameterName = "dbProvider",
                Type = $"I{_dataLayerKProject.ProjectName}DbProvider"
            });
            constructor.CodeSnippet = "DbProvider = dbProvider ?? throw new ArgumentException(nameof(dbProvider));";
            dataService.Constructor.Add(constructor);


            return dataService;
        }

        

    }
}
