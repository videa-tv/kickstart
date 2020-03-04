using System;
using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject.Table;
using Kickstart.Pass2.SqlServer;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject
{
    public class RelationalDataLayerServiceProjectService : DataLayerServiceProjectServiceBase, IDataLayerServiceProjectService, IRelationalDataLayerServiceProjectService
    {
        //KDataStoreProject _dataStoreKProject;
        //private KDataLayerProject _dataLayerKProject;
        //private CProject _dataProject;
        //private List<CStoredProcedure> _storedProcedures;
        //private IEnumerable<KTableType> _tableTypes;
        public string ConnectionString { get; set; }

        public DataStoreTypes ConnectsToDatabaseType { get; set; } = DataStoreTypes.SqlServer;

        public RelationalDataLayerServiceProjectService()
        {
            int x = 1;
        }

        public CProject BuildProject(KDataStoreProject databaseKProject, KDataLayerProject dataLayerKProject,
            IEnumerable<CStoredProcedure> storedProceduresIn,
            IEnumerable<KTable> tables, IEnumerable<KTableType> tableTypes, IEnumerable<CView> views)
        {
            _dataStoreKProject = databaseKProject;
            _dataLayerKProject = dataLayerKProject;
            var storedProcedures = databaseKProject.StoredProcedure.Select(s => s.GeneratedStoredProcedure).ToList();

            _storedProcedures = storedProcedures;
            _tables = tables;
            _tableTypes = tableTypes;

            var dataProject = new CProject
            {
                ProjectName = $"{dataLayerKProject.ProjectFullName}",
                ProjectShortName = dataLayerKProject.ProjectShortName,
                ProjectFolder = $"{dataLayerKProject.ProjectFolder}.{dataLayerKProject.ConnectsToDatabaseType}",
                ProjectType = CProjectType.CsProj,
                ProjectIs = CProjectIs.DataAccess
            };

            _dataProject = dataProject; //todo: is the proper?
            dataProject.TemplateProjectPath =
                @"templates\NetStandard20ClassLibrary.csproj";
            var iDbDiagnosticsFactoryInterface = BuildIDbDiagnosticsFactoryInterface(dataProject);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Interface = iDbDiagnosticsFactoryInterface,
                File = new CFile
                {
                    Folder = "Diagnostics",
                    FileName = $"{iDbDiagnosticsFactoryInterface.InterfaceName}.cs"
                }
            });



            var dbProviderInterface = BuildDbProviderInterface();
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Interface = dbProviderInterface,
                File = new CFile {Folder = "Providers", FileName = $"{dbProviderInterface.InterfaceName}.cs"}
            });


            var dbProvider = BuildDbProviderClass(dbProviderInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = dbProvider,
                File = new CFile {Folder = "Providers", FileName = $"{dbProvider.ClassName}.cs"}
            });

            var baseDataService = BuildBaseDataServiceClass(dbProviderInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = baseDataService,
                File = new CFile {Folder = "DataServices\\Base", FileName = $"{baseDataService.ClassName}.cs"}
            });

            if (tableTypes != null)
            {
                if (ConnectsToDatabaseType == DataStoreTypes.SqlServer)
                {
                    //only need these for Sql Server
                    var tableValueTypes = BuildTableTypeLists(tableTypes);
                    foreach (var tableType in tableValueTypes)
                        dataProject.ProjectContent.Add(new CProjectContent
                        {
                            BuildAction = CBuildAction.DoNotInclude,
                            Class = tableType,
                            File = new CFile {Folder = "Types", FileName = $"{tableType.ClassName}.cs"}
                        });
                }

                var dtoClassesAll = BuildEntityClasses(storedProcedures, tables,  tableTypes, views, dataProject);

                if (dataLayerKProject.OnePerTable)
                {
                    foreach (var @class in dtoClassesAll)
                    {
                        var dtoClasses = new List<CClass> {@class};

                        var dataServiceInterface = BuildDataServiceInterface(dtoClasses);
                        dataLayerKProject.DataServiceInterface.Add(dataServiceInterface);
                        dataProject.ProjectContent.Add(new CProjectContent
                        {
                            BuildAction = CBuildAction.DoNotInclude,
                            Interface = dataServiceInterface,
                            File = new CFile
                            {
                                Folder = "DataServices",
                                FileName = $"{@class.ClassName}{dataServiceInterface.InterfaceName}.cs"
                            }
                        });

                        var dataService = BuildDataServiceClass(dataServiceInterface, dbProviderInterface,
                            iDbDiagnosticsFactoryInterface, dtoClasses);
                        dataProject.ProjectContent.Add(new CProjectContent
                        {
                            BuildAction = CBuildAction.DoNotInclude,
                            Class = dataService,
                            File = new CFile
                            {
                                Folder = "DataServices",
                                FileName = $"{@class.ClassName}{dataService.ClassName}.cs"
                            }
                        });
                    }

                }
                else
                {
                    var dataServiceInterface = BuildDataServiceInterface(dtoClassesAll);
                    dataLayerKProject.DataServiceInterface.Add(dataServiceInterface);
                    dataProject.ProjectContent.Add(new CProjectContent
                    {
                        BuildAction = CBuildAction.DoNotInclude,
                        Interface = dataServiceInterface,
                        File = new CFile
                        {
                            Folder = "DataServices",
                            FileName = $"{dataServiceInterface.InterfaceName}.cs"
                        }
                    });

                    var dataService = BuildDataServiceClass(dataServiceInterface, dbProviderInterface,
                        iDbDiagnosticsFactoryInterface, dtoClassesAll);
                    dataProject.ProjectContent.Add(new CProjectContent
                    {
                        BuildAction = CBuildAction.DoNotInclude,
                        Class = dataService,
                        File = new CFile {Folder = "DataServices", FileName = $"{dataService.ClassName}.cs"}
                    });
                }
            }

            var dataHealthCheckInterface = BuildDataHealthCheckInterface();
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Interface = dataHealthCheckInterface,
                File = new CFile {Folder = "DataServices", FileName = $"{dataHealthCheckInterface.InterfaceName}.cs"}
            });

            var dataHealthCheckClass =
                BuildDataHealthCheckClass(dataHealthCheckInterface, baseDataService, dbProviderInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = dataHealthCheckClass,
                File = new CFile {Folder = "DataServices", FileName = $"{dataHealthCheckClass.ClassName}.cs"}
            });


            var dbDiagnosticsFactoryClass = BuildDbDiagnosticsFactoryClass(dataProject, iDbDiagnosticsFactoryInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = dbDiagnosticsFactoryClass,
                File = new CFile {Folder = "Diagnostics", FileName = $"{dbDiagnosticsFactoryClass.ClassName}.cs"}
            });

            var iDbDiagnosticsHandlerInterface = BuildIDbDiagnosticsHandlerInterface(dataProject);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Interface = iDbDiagnosticsHandlerInterface,
                File = new CFile
                {
                    Folder = "Diagnostics",
                    FileName = $"{iDbDiagnosticsHandlerInterface.InterfaceName}.cs"
                }
            });

            var dbDiagnosticsHandlerClass = BuildDbDiagnosticsHandlerClass(dataProject, iDbDiagnosticsHandlerInterface);
            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.DoNotInclude,
                Class = dbDiagnosticsHandlerClass,
                File = new CFile {Folder = "Diagnostics", FileName = $"{dbDiagnosticsHandlerClass.ClassName}.cs"}
            });

            AddEmbeddedQueryExtension(dataProject);
            AddStoredProcsAsEmbeddedQuery(databaseKProject, dataProject);
            dataProject.NuGetReference.Add(new CNuGet {NuGetName = "Dapper", Version = "1.50.5"});
            dataProject.NuGetReference.Add(new CNuGet {NuGetName = "Company.Datastore", Version = "2.0.0"});


            if (ConnectsToDatabaseType == DataStoreTypes.SqlServer)
            {
                dataProject.NuGetReference.Add(new CNuGet {NuGetName = "System.Data.SqlClient", Version = "4.4.0"});
            }
            else if (ConnectsToDatabaseType == DataStoreTypes.Postgres)
            {
                dataProject.NuGetReference.Add(new CNuGet {NuGetName = "Npgsql", Version = "4.0.1"});
            }
            else if (ConnectsToDatabaseType == DataStoreTypes.MySql)
            {
                dataProject.NuGetReference.Add(new CNuGet {NuGetName = "MySql.Data", Version = "6.10.6"});
            }
            else
            {
                throw new NotImplementedException();
            }

            return dataProject;
        }

        private CClass BuildDbDiagnosticsHandlerClass(CProject dataProject, CInterface iDbDiagnosticsHandlerInterface)
        {
            var @class = new CClass("DbDiagnosticsHandler")
            {
                Implements = new List<CInterface>()
                {
                    iDbDiagnosticsHandlerInterface
                },
                Namespace = new CNamespace {NamespaceName = iDbDiagnosticsHandlerInterface.Namespace.NamespaceName}
            };

            @class.NamespaceRef.Add(new CNamespaceRef() {ReferenceTo = new CNamespace() {NamespaceName = "System"}});

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "Start"
            });
            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "Complete"
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
                    }
                }
            );

            @class.Method.Add(new CMethod()
            {
                UseExpressionDefinition = true,
                ReturnType = "IDbDiagnosticsHandler",
                MethodName = "WithTag",
                Parameter = new List<CParameter>()
                {
                    new CParameter() {Type = "string", ParameterName = "key"},
                    new CParameter() {Type = "string", ParameterName = "value"}
                },
                CodeSnippet = "this;"
            });

            return @class;
        }

        private CInterface BuildIDbDiagnosticsHandlerInterface(CProject dataProject)
        {
            var @interface = new CInterface()
            {
                InterfaceName = "IDbDiagnosticsHandler",
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.{_dataLayerKProject.ProjectSuffix}.Diagnostics"

                }
            };
            @interface.NamespaceRef.Add(new CNamespaceRef {ReferenceTo = new CNamespace {NamespaceName = "System"}});
    
            @interface.Method.Add(new CMethod
            {
                SignatureOnly = true,
                ReturnType = "void",
                MethodName = "Start"
            });

            @interface.Method.Add(new CMethod
            {
                SignatureOnly = true,
                ReturnType = "void",
                MethodName = "Complete"
            });

            @interface.Method.Add(new CMethod
            {
                SignatureOnly = true,
                ReturnType = "void",
                MethodName = "CompleteError",
                Parameter = new List<CParameter>()
                {
                    new CParameter()
                    {
                        Type = "Exception",
                        ParameterName = "ex"
                    }
                }
            });

            @interface.Method.Add(new CMethod
            {
                SignatureOnly = true,
                ReturnType = "IDbDiagnosticsHandler",
                MethodName = "WithTag",
                Parameter = new List<CParameter>()
                {
                    new CParameter()
                    {
                        Type = "string",
                        ParameterName = "key"
                    },
                    new CParameter()
                    {
                        Type = "string",
                        ParameterName = "value"
                    }
                }
            });


            return @interface;
        }

        private CClass BuildDbDiagnosticsFactoryClass(CProject dataProject, CInterface iDbDiagnosticsHandler)
        {
            var @class = new CClass("DbDiagnosticsFactory")
            {
                Implements = new List<CInterface>()
                {
                    iDbDiagnosticsHandler
                },
                Namespace = new CNamespace {NamespaceName = iDbDiagnosticsHandler.Namespace.NamespaceName}
            };

            @class.Method.Add(new CMethod()
            {
                UseExpressionDefinition = true,
                AccessModifier = CAccessModifier.Public,
                ReturnType = "IDbDiagnosticsHandler",
                MethodName = "CreateHandler",
                Parameter = new List<CParameter>()
                {
                    new CParameter(){ Type = "string", ParameterName = "name"} 
                },
                CodeSnippet = "new DbDiagnosticsHandler();"
            });

            return @class;
        }

        public override CInterface BuildIDbDiagnosticsFactoryInterface(CProject dataProject)
        {
            var @interface = new CInterface()
            {
                InterfaceName = "IDbDiagnosticsFactory",
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.{_dataLayerKProject.ProjectSuffix}.Diagnostics"

                }
            };

            @interface.Method.Add(new CMethod()
            {
                SignatureOnly = true,
                ReturnType = "IDbDiagnosticsHandler",
                MethodName = "CreateHandler",
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = "string", ParameterName = "name"}
                }
            });

            return @interface;
        }

        private CClass BuildDataHealthCheckClass(CInterface dataHealthCheckInterface, CClass baseDataService, CInterface dbProviderInterface)
        {
            var @class = new CClass ("DataHealthCheckService")
            {
                AccessModifier = CAccessModifier.Public,
                Implements = new List<CInterface>() { dataHealthCheckInterface } ,
                InheritsFrom = baseDataService };

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Dapper" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = baseDataService.Namespace });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = dbProviderInterface.Namespace });


            @class.Namespace = new CNamespace { NamespaceName = dataHealthCheckInterface.Namespace.NamespaceName };

            var constructor = new CConstructor { AccessModifier = CAccessModifier.Public, ConstructorName = "DataHealthCheckService", Parameter = new List<CParameter>
            {
                new CParameter { Type= dbProviderInterface.InterfaceName,  ParameterName = "dbProvider", PassToBaseClass = true },
               
            } };

            @class.Constructor.Add(constructor);

            var methodCheck = new CMethod { ReturnType="Task<bool>", MethodName = "Check", IsAsync = true, AccessModifier = CAccessModifier.Public };

            methodCheck.CodeSnippet =
                @"using var connection = DbProvider.GetConnection();
                return await connection.QueryFirstAsync<bool>(""select 1"").ConfigureAwait(false);
                ";

            @class.Method.Add(methodCheck);

            return @class;
        }

        

        private void AddEmbeddedQueryExtension(CProject dataProject)
        {
            var @class = new CClass("AssemblyExtensions")
            {
                IsStatic = true,
                Namespace = new CNamespace { NamespaceName = $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Extensions" }
            };

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Reflection" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.IO" }
            });


            var method = new CMethod { IsStatic =true, MethodName = "GetEmbeddedQuery", IsExtensionMethod = true, ReturnType = "string" };
            method.Parameter.Add(new CParameter { Type="Assembly", ParameterName = "source" });
            method.Parameter.Add(new CParameter { Type = "string", ParameterName = "sqlQueryName" });

            @class.Method.Add(method);

            method.CodeSnippet =
                $@" using (var stream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream($""{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.EmbeddedSql.{{sqlQueryName}}.esql""))
            using (var reader = new StreamReader(stream))
            {{
                return reader.ReadToEnd();
            }}
            ";


            dataProject.ProjectContent.Add(new CProjectContent
            {
                BuildAction = CBuildAction.None,
                Class = @class,
                File = new CFile { Folder = "Extensions", FileName = $"AssemblyExtensions.cs" }
            });
        }

        private void AddStoredProcsAsEmbeddedQuery(KDataStoreProject databaseKProject, CProject dataProject)
        {
            

            foreach (var sp in databaseKProject.StoredProcedure)
            {
                if (!sp.GeneratedStoredProcedure.GenerateAsEmbeddedQuery)
                    continue;
                var sql = sp.GeneratedStoredProcedure.StoredProcedureBody;
                
                if (ConnectsToDatabaseType == DataStoreTypes.MySql ||
                    ConnectsToDatabaseType == DataStoreTypes.Postgres)
                {
                    sql = $"SELECT 'TODO: Convert this sql: {sql}'";
                }
                var query = new CText()
                {
                    Text = sql
                };

                dataProject.ProjectContent.Add(new CProjectContent
                {
                    BuildAction = CBuildAction.EmbeddedResource,
                    Content = query,
                    File = new CFile { Folder = "EmbeddedSql", FileName = $"{sp.StoredProcedureName}.esql" }
                });
            }

           
        }


        private CClass BuildDataServiceClass(CInterface dataServiceInterface, CInterface dataProviderInterface, CInterface iDbDiagnosticsFactoryInterface,  List<CClass> dtoClasses)
        {
            var dataService = new CClass($"{_dataLayerKProject.ProjectNameAsClassNameFriendly}{_dataLayerKProject.ProjectSuffix}Service")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.{_dataLayerKProject.ProjectSuffix}.DataServices"
                },
                IsAbstract = false,
                InheritsFrom = new CClass ("BaseDataService"),
                Implements = new List<CInterface> {new CInterface {InterfaceName = dataServiceInterface.InterfaceName}}
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
                ReferenceTo = new CNamespace {NamespaceName = "System.Linq"}
            });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Reflection" } });

            dataService.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "System.Threading.Tasks"}
            });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace {NamespaceName = "Dapper"}});
            
            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.DataServices.Base" } });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Extensions" } });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities" } });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = dataProviderInterface.Namespace });

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = iDbDiagnosticsFactoryInterface .Namespace});


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
                CodeSnippet = "_diagnosticsFactory = diagnosticsFactory;"
            };

            constructor.Parameter.Add(new CParameter
            {
                PassToBaseClass = true,
                ParameterName = "dbProvider",
                Type = $"I{_dataLayerKProject.ProjectNameAsClassNameFriendly}DbProvider"
            });

            constructor.Parameter.Add(new CParameter
            {
                PassToBaseClass = false,
                ParameterName = "diagnosticsFactory",
                Type = $"IDbDiagnosticsFactory"
            });


            dataService.Constructor.Add(constructor);

            dataService.Field.Add(new CField()
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "IDbDiagnosticsFactory",
                FieldName = "_diagnosticsFactory"
            });

            var methods = GetDataServiceMethods();

            dataService.Method.AddRange(methods);


            //todo: clean this up
            if (dtoClasses.Count > 0)
                dataService.NamespaceRef.Add(_dataProject.BuildNamespaceRefForType(dtoClasses.First().ClassName));
            return dataService;
        }
        private List<CMethod> GetDataServiceMethodsFromStoredProcs()
        {
            var methods = new List<CMethod>();
            foreach (var storedProcedure in _dataStoreKProject.StoredProcedure.Select(s=>s.GeneratedStoredProcedure))
            {
                var dtoParameterClass = BuildParameterEntityClass(storedProcedure, storedProcedure.ParameterSetName);
                var dtoResultClass = BuildResultEntityClass(storedProcedure, storedProcedure.ResultSetName, _dataLayerKProject);

                var storedProcName = storedProcedure.StoredProcedureName;
                var resultClassName = dtoResultClass.ClassName;
                var schemaName = storedProcedure.Schema.SchemaName;

                if (storedProcedure.ConvertToSnakeCase)
                {
                    resultClassName = resultClassName.ToSnakeCase();
                    storedProcName = storedProcName.ToSnakeCase();
                    schemaName = schemaName.ToSnakeCase();
                }


 
                var method = new CMethod
                {
                    AccessModifier = CAccessModifier.Public,
                    IsAsync = true,
                    ReturnType = storedProcedure.HasResultSet
                        ? $"Task<IEnumerable<{dtoResultClass.ClassName}>>"
                        : "Task<bool>",
                    MethodName = $"{storedProcedure.StoredProcedureName}Async",
                    DerivedFrom = storedProcedure
                };

                method.Parameter.AddRange(BuildMethodParameters(dtoParameterClass,  storedProcedure));

                var codeWriter = new CodeWriter();
                WriteDiagnosticsSetupCode(codeWriter, storedProcedure, method);
                codeWriter.WriteLine();
                codeWriter.WriteLine("IDbConnection connection = null;");
                codeWriter.WriteLine();
                codeWriter.WriteLine("try");
                codeWriter.WriteLine("{");
                codeWriter.Indent();

 
                if (storedProcedure.GenerateAsEmbeddedQuery)
                {
                    var storedProcNameOriginal = storedProcedure.StoredProcedureName;

                    codeWriter.WriteLine($@"var sqlQuery = Assembly.GetExecutingAssembly().GetEmbeddedQuery(""{storedProcNameOriginal}"");");
                }
                codeWriter.WriteLine();
                codeWriter.WriteLine("diagHandler.Start();");
                codeWriter.WriteLine();
                codeWriter.WriteLine("connection = DbProvider.GetConnection();");
                codeWriter.WriteLine();

                WriteParameterSetCode(codeWriter, dtoParameterClass, storedProcedure);


                if (storedProcedure.HasResultSet)
                {
                    WriteReturnResultSetCode(codeWriter, dtoResultClass, storedProcedure, schemaName, storedProcName);
                }
                else
                {
                    WriteReturnBoolenCode(codeWriter, storedProcedure, schemaName, storedProcName);
                }
                codeWriter.Unindent();
                codeWriter.WriteLine(@"}");
                codeWriter.WriteLine("catch (Exception e)");
                codeWriter.WriteLine("{");
                codeWriter.Indent();
                codeWriter.WriteLine("diagHandler.CompleteError(e);");
                codeWriter.WriteLine("throw;");
                codeWriter.Unindent();
                codeWriter.WriteLine("}");
                codeWriter.WriteLine("finally");
                codeWriter.WriteLine("{");
                codeWriter.Indent();
                codeWriter.WriteLine("connection?.Dispose();");
                codeWriter.Unindent();
                codeWriter.WriteLine("}");

                method.CodeSnippet = codeWriter.ToString();
                methods.Add(method);
            }
            return methods;
        }

        private IEnumerable<CParameter> BuildMethodParameters(CClass dtoParameterClass,  CStoredProcedure storedProcedure)
        {
            var parameters = new List<CParameter>();
            if (dtoParameterClass != null)
            {
                var methodParameter = new CParameter
                {
                    Type = dtoParameterClass.ClassName,
                    ParameterName = dtoParameterClass.ClassName.ToLower() //.ParameterNameCamelCase
                };
                parameters.Add(methodParameter);
            }
            else
            {
                foreach (var parameter in storedProcedure.Parameter)
                {
                    var type = "object";
                    if (!parameter.ParameterTypeIsUserDefined)
                    {
                        type = SqlMapper.ParseValueAsSqlDbType(parameter.ParameterTypeRaw).ToClrTypeName();

                        if (parameter.IsCollection)
                        {
                            type = $"IEnumerable<{type}>";
                        }
                    }
                    else if (parameter.ParameterTypeIsUserDefined)
                    {
                        var parameterType = parameter.ParameterTypeRaw;
                        if (parameterType.StartsWith("tt"))
                        {
                            parameterType = parameterType.Substring(2, parameterType.Length - 2);
                        }
                        var tableDto = FindTableByParameterTypeRaw(parameterType);
                        
                        type = GetTableDto(_tables, tableDto, tableDto.TableName).ClassName;
                        type = $"IEnumerable<{type}>";

                        //todo: need to create mapping for Npgsql from Dto class to Postgresql type
                        /*
                        var tableType = FindTableType(parameter.ParameterTypeRaw);
                        //var tableTypeClass = BuildTableTypeList(tableType);
                        //type = $"{tableTypeClass.Field.First().FieldType}";
                        type = GetTableTypeDto(tableType.GeneratedTableType, tableType.TableTypeName).ClassName;
                        type = $"IEnumerable<{type}>";
                        */
                    }

                    var methodParameter = new CParameter
                    {
                        Type = type,
                        ParameterName = parameter.ParameterNameCamelCase
                    };
                    parameters.Add(methodParameter);
                }
            }

            return parameters;
        }

        private static void WriteReturnBoolenCode(CodeWriter codeWriter, CStoredProcedure storedProcedure, string schemaName,
            string storedProcName)
        {
            codeWriter.WriteLine($@"var result = await connection");
            codeWriter.WriteLine($@"     .ExecuteAsync(");
            if (storedProcedure.GenerateAsEmbeddedQuery)
            {
                codeWriter.WriteLine(
                    $@"     sqlQuery,");
                codeWriter.WriteLine($@"        sqlParams,");

                codeWriter.WriteLine($@"        commandType: CommandType.Text)");
            }
            else
            {
                codeWriter.WriteLine(
                    $@"     ""{schemaName}.{storedProcName}"",");
                codeWriter.WriteLine($@"        sqlParams,");

                codeWriter.WriteLine($@"        commandType: CommandType.StoredProcedure)");
            }

            codeWriter.WriteLine($@"     .ConfigureAwait(false);");
            codeWriter.WriteLine("");
            codeWriter.WriteLine($@"return result > 0;");
        }

        private static void WriteReturnResultSetCode(CodeWriter codeWriter, CClass dtoResultClass,
            CStoredProcedure storedProcedure, string schemaName, string storedProcName)
        {
            codeWriter.WriteLine($@"var resultSet = await connection");
            codeWriter.WriteLine($@"     .QueryAsync<{dtoResultClass.ClassName}>(");

            if (storedProcedure.GenerateAsEmbeddedQuery)
            {
                codeWriter.WriteLine(
                    $@"     sqlQuery,");
                codeWriter.WriteLine($@"        sqlParams,");

                codeWriter.WriteLine($@"        commandType: CommandType.Text)");
            }
            else
            {
                codeWriter.WriteLine(
                    $@"     ""{schemaName}.{storedProcName}"",");
                codeWriter.WriteLine($@"        sqlParams,");

                codeWriter.WriteLine($@"        commandType: CommandType.StoredProcedure)");
            }

            codeWriter.WriteLine($@"     .ConfigureAwait(false);");
            codeWriter.WriteLine("");
            codeWriter.WriteLine(" var result = resultSet.ToList();");
            codeWriter.WriteLine();
            codeWriter.WriteLine("diagHandler.Complete();");
            codeWriter.WriteLine();
            codeWriter.WriteLine($@"return result;");
        }

        private void WriteParameterSetCode(CodeWriter codeWriter, CClass dtoParameterClass, CStoredProcedure storedProcedure)
        {
            codeWriter.WriteLine("var sqlParams = new");
            codeWriter.WriteLine("{");
            codeWriter.Indent();
            if (dtoParameterClass != null)
            {
                bool first = true;
                foreach (var property in dtoParameterClass.Property)
                {
                    if (!first)
                    {
                        codeWriter.WriteLine(",");
                    }

                    first = false;
                    var propertyName = property.PropertyName;
                    if (storedProcedure.ConvertToSnakeCase)
                    {
                        propertyName = propertyName.ToSnakeCase();
                    }

                    var dbType = SqlMapper.GetDbType(property.Type);
                    codeWriter.Write($"{property.PropertyName}");

                    /*codeWriter.WriteLine(
                        $@"sqlParams.Add(""@{propertyName}"", {dtoParameterClass.ClassName.ToLower()}.{
                                property.PropertyName
                            }, DbType.{dbType});");*/
                }
            }
            else
            {
                bool first = true;
                foreach (var parameter in storedProcedure.Parameter)
                {
                    if (!first)
                    {
                        codeWriter.WriteLine(",");
                    }

                    first = false;
                    var parameterName = parameter.ParameterName;
                    if (storedProcedure.ConvertToSnakeCase)
                        parameterName = parameterName.ToSnakeCase();

                    if (parameter.ParameterTypeIsUserDefined)
                    {
                        //var tableType = FindTableType(parameter.ParameterTypeRaw);
                        //var tableTypeClass = BuildTableTypeList(tableType);
                        codeWriter.Write($"{parameter.ParameterNameCamelCase}");

                        /*
                        codeWriter.WriteLine(
                            $@"sqlParams.Add(""@{parameterName}"", new {tableTypeClass.ClassName}({
                                    parameter.ParameterNameCamelCase
                                }), DbType.Object);");
                                */
                    }
                    else
                    {
                        var dbType = parameter.ParameterType.ToClrType().ToDbType();
                        codeWriter.Write($"{parameter.ParameterNameCamelCase}");
                        /*
                        codeWriter.WriteLine(
                            $@"sqlParams.Add(""@{parameterName}"", {
                                    parameter.ParameterNameCamelCase
                                }, DbType.{dbType});");
                                */
                    }
                }
            }
            codeWriter.WriteLine();
            codeWriter.Unindent();
            codeWriter.WriteLine("};");
            codeWriter.WriteLine("");
        }

        private static void WriteDiagnosticsSetupCode(CodeWriter codeWriter, CStoredProcedure storedProcedure, CMethod method)
        {
            codeWriter.WriteLine("var diagHandler = _diagnosticsFactory");
            codeWriter.Write($@".CreateHandler(""{storedProcedure.StoredProcedureName}"")");
            codeWriter.Indent();
            int pCount = 0;

            var parameters = method.Parameter.Where(p => p.IsScalar).ToList();
            if (parameters.Count > 0)
            {
                codeWriter.WriteLine();
                foreach (var cParameter in parameters)
                {
                    var parameterName = cParameter.ParameterName;
                    if (cParameter.Type != "string")
                    {
                        parameterName += ".ToString()";
                    }

                    codeWriter.Write($".WithTag(nameof({cParameter.ParameterName}), {parameterName})");
                    if (pCount == parameters.Count - 1)
                    {
                        codeWriter.WriteLine(";");
                    }
                    else
                    {
                        codeWriter.WriteLine();
                    }

                    pCount++;
                }
            }
            else
            {
                codeWriter.WriteLine(";");
            }
            codeWriter.Unindent();
        }

        private List<CMethod> GetDataServiceMethodsFromQueries()
        {
            var methods = new List<CMethod>();
            foreach (var kQuery in _dataStoreKProject.Query)
            {
                if (kQuery.GeneratedQuery == null)
                {
                    //todo: shouldn't have any null
                    continue;
                }
                var query = kQuery.GeneratedQuery;

                CClass dtoParameterClass = null;// GetParameterDto(query, query.ParameterSetName);
                CClass dtoResultClass = null;//GetResultDto(query, query.ResultSetName);

                var method = new CMethod
                {
                    AccessModifier = CAccessModifier.Public,
                    IsAsync = true,
                    ReturnType = query.HasResultSet
                        ? $"Task<IEnumerable<{dtoResultClass.ClassName}>>"
                        : "Task<bool>",
                    MethodName = $"{query.QueryName}Async",
                    //DerivedFrom = query
                };
                
                if (dtoParameterClass != null)
                {
                    var methodParameter = new CParameter
                    {
                        Type = dtoParameterClass.ClassName,
                        ParameterName = dtoParameterClass.ClassName.ToLower() //.ParameterNameCamelCase
                    };
                    method.Parameter.Add(methodParameter);
                }
                else
                {
                    foreach (var parameter in query.Parameter)
                    {
                        var type = "object";
                        if (!parameter.ParameterTypeIsUserDefined)
                        {
                            type = SqlMapper.ParseValueAsSqlDbType(parameter.ParameterTypeRaw).ToClrTypeName();

                            if (parameter.IsCollection)
                            {
                                type = $"IEnumerable<{type}>";
                            }
                        }
                        else if (parameter.ParameterTypeIsUserDefined)
                        {
                            var parameterType = parameter.ParameterTypeRaw;
                            if (parameterType.StartsWith("tt"))
                            {
                                parameterType = parameterType.Substring(2, parameterType.Length - 2);
                            }
                            var tableDto = FindTableByParameterTypeRaw(parameterType);

                            type = GetTableDto(_tables, tableDto, tableDto.TableName).ClassName;
                            type = $"IEnumerable<{type}>";
                            /*
                            var tableType = FindTableType(parameter.ParameterTypeRaw);
                           
                            type = GetTableTypeDto(tableType.GeneratedTableType, tableType.TableTypeName).ClassName;
                            type = $"IEnumerable<{type}>";
                            */
                        }

                        
                        var methodParameter = new CParameter
                        {
                            Type = type,
                            ParameterName = parameter.ParameterNameCamelCase
                        };
                        method.Parameter.Add(methodParameter);
                    }
                }

                var codeWriter = new CodeWriter();
                codeWriter.WriteLine($@"using var connection = DbProvider.GetConnection();");
                codeWriter.WriteLine("");
                codeWriter.WriteLine("var sqlParams = new DynamicParameters();");

                if (dtoParameterClass != null)
                    foreach (var property in dtoParameterClass.Property)
                    {
                        var propertyName = property.PropertyName;
                        /*
                        if (ConnectsToDatabaseType == DatabaseTypes.Postgres)
                            propertyName = propertyName.ToSnakeCase();
                            */
                        var dbType = SqlMapper.GetDbType(property.Type);
                        codeWriter.WriteLine(
                            $@"sqlParams.Add(""@{propertyName}"",{dtoParameterClass.ClassName.ToLower()}.{
                                    property.PropertyName
                                }, DbType.{dbType});");
                    }
                else
                    foreach (var parameter in query.Parameter)
                    {
                        var parameterName = parameter.ParameterName;
                        /*
                        if (ConnectsToDatabaseType == DatabaseTypes.Postgres)
                            parameterName = parameterName.ToSnakeCase();
                            */
                        if (parameter.ParameterTypeIsUserDefined)
                        {
                            var tableType = FindTableType(parameter.ParameterTypeRaw);
                            var tableTypeClass = BuildTableTypeList(tableType);

                           
                            codeWriter.WriteLine(
                                $@"sqlParams.Add(""@{parameterName}"",new {tableTypeClass.ClassName}({
                                        parameter.ParameterNameCamelCase
                                    }), DbType.Object);");
                        }
                        else
                        {
                            var dbType = parameter.ParameterType.ToClrType().ToDbType();
                            codeWriter.WriteLine(
                                $@"sqlParams.Add(""@{parameter.ParameterName}"",{
                                        parameter.ParameterNameCamelCase
                                    }, DbType.{dbType});");
                        }
                    }
                        
                codeWriter.WriteLine("");
                if (query.HasResultSet)
                {
                    var schemaName = query.Schema.SchemaName;
                    var queryName = query.QueryName;
                    var resultClassName = dtoResultClass.ClassName;
                    /*
                    if (ConnectsToDatabaseType == DatabaseTypes.Postgres)
                    {
                        schemaName = schemaName.ToSnakeCase();
                        queryName = queryName.ToSnakeCase();
                        resultClassName = resultClassName.ToSnakeCase();
                    }*/
                    codeWriter.WriteLine($@"var result = await connection");
                    codeWriter.WriteLine($@"     .QueryAsync<{dtoResultClass.ClassName}>(");
                    codeWriter.WriteLine(
                        $@"     ""{schemaName}.{queryName}"",");
                    codeWriter.WriteLine($@"        sqlParams,");
                    codeWriter.WriteLine($@"        commandType: CommandType.StoredProcedure)");
                    codeWriter.WriteLine($@"     .ConfigureAwait(false);");
                    codeWriter.WriteLine("");
                    codeWriter.WriteLine($@"return result.ToList();");
                }
                else
                {
                    codeWriter.WriteLine($@"var result = await connection");
                    codeWriter.WriteLine($@"     .ExecuteAsync(");
                    codeWriter.WriteLine(
                        $@"     ""{query.QueryName}"",");
                    codeWriter.WriteLine($@"        sqlParams,");
                    codeWriter.WriteLine($@"        commandType: CommandType.StoredProcedure)");
                    codeWriter.WriteLine($@"     .ConfigureAwait(false);");
                    codeWriter.WriteLine("");
                    codeWriter.WriteLine($@"return result > 0;");
                }
                codeWriter.WriteLine(@"");
                method.CodeSnippet = codeWriter.ToString();
                methods.Add(method);
                
            }
            return methods;
        }
        private List<CMethod> GetDataServiceMethods()
        {
            var methods = new List<CMethod>();
            if (_dataLayerKProject.KickstartBulkStore)
                methods.Add(BuildBulkStoreMethod());

            methods.AddRange(GetDataServiceMethodsFromStoredProcs());
            methods.AddRange(GetDataServiceMethodsFromQueries());
            return methods;
        }

        
        protected override CMethod BuildBulkStoreMethod()
        {
            var bulkStoreMethod = new CMethod
            {
                ReturnType = "Task<bool>",
                IsAsync = true,
                SignatureOnly = false,
                MethodName = $"{_dataLayerKProject.BulkStoreMethodName}"
            };
            bulkStoreMethod.CodeSnippet =
                "//Todo: call stored procedure for each item (or chunk) in each collection, in proper order, based on DBMS ForeignKeys\r\n throw new NotImplementedException();";
            //create a "Stored proc" so later code works
            var storedProcList = new CStoredProcList();

            foreach (var storedProcedure in _storedProcedures)
                if (!string.IsNullOrEmpty(storedProcedure.ParameterSetName))
                {
                    bulkStoreMethod.Parameter.Add(new CParameter
                    {
                        Type = $"IEnumerable<{storedProcedure.ParameterSetName}>",
                        ParameterName = storedProcedure.ParameterSetName
                    });
                    storedProcList.List.Add(storedProcedure);
                }

            var codeWriter = new CodeWriter();
            codeWriter.WriteLine("//Todo: pass to stored procs in chunks, not row by row");
            codeWriter.WriteLine("//Todo: call in order of foreign keys");
            codeWriter.WriteLine("//Todo: replace any 'StageID's with real foreign keys");
            codeWriter.WriteLine();
            foreach (var storedProcedure in _storedProcedures)
                if (!string.IsNullOrEmpty(storedProcedure.ParameterSetName))
                {
                    codeWriter.WriteLine($"foreach (var item in {storedProcedure.ParameterSetName})");
                    codeWriter.WriteLine("{");
                    codeWriter.Indent();
                    codeWriter.WriteLine($"await {storedProcedure.StoredProcedureName}(item);");
                    codeWriter.Unindent();
                    codeWriter.WriteLine("}");
                }
            codeWriter.WriteLine();

            if (!_storedProcedures.Any())
            {
                codeWriter.WriteLine("throw new NotImplementedException();");
            }
            else
            {
                codeWriter.WriteLine("return true;");
            }
            bulkStoreMethod.CodeSnippet = codeWriter.ToString();
            bulkStoreMethod.DerivedFrom = storedProcList;
            return bulkStoreMethod;
        }

        private CInterface BuildDbProviderInterface()
        {
            var dbProviderInterface = new CInterface();

            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "System.Data"}
            });

            /*
            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "System.Data.SqlClient"}
            });
            */
            dbProviderInterface.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.Datastore.Provider" }
            });

            dbProviderInterface.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Providers"
            };

            dbProviderInterface.InterfaceName = $"I{_dataLayerKProject.ProjectNameAsClassNameFriendly}DbProvider";
            dbProviderInterface.InheritsFrom = new CInterface() {  InterfaceName = "IDbProvider" };
            /*
            dbProviderInterface.Method.Add(new CMethod
            {
                SignatureOnly = true,
                MethodName = "GetConnection",
                ReturnType = "IDbConnection"
            });*/

            return dbProviderInterface;
        }

        private CClass BuildDbProviderClass(CInterface dbProviderInterface)
        {
            var codeSnippet = string.Empty;
            if (ConnectsToDatabaseType == DataStoreTypes.SqlServer)
                codeSnippet = "return new SqlConnection(_connectionString);";
            else if (ConnectsToDatabaseType == DataStoreTypes.Postgres)
            {
                codeSnippet = "return new NpgsqlConnection(_connectionString);";
            }
            else if (ConnectsToDatabaseType == DataStoreTypes.MySql)
            {
                codeSnippet = "return new MySqlConnection(_connectionString);";
            }
            else
            {
                throw new NotImplementedException();
            }
            var dbProvider = new CClass($"{_dataLayerKProject.ProjectNameAsClassNameFriendly}DbProvider");
            dbProvider.Implements.Add(dbProviderInterface);

            dbProvider.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });

            dbProvider.NamespaceRef.Add(
                new CNamespaceRef {ReferenceTo = new CNamespace {NamespaceName = "System.Data"}});

            if (_dataLayerKProject.ConnectsToDatabaseType == DataStoreTypes.SqlServer)
            {
                dbProvider.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = "System.Data.SqlClient" }
                });
            }
            else if (ConnectsToDatabaseType == DataStoreTypes.Postgres)
            {
                dbProvider.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = "Npgsql" }
                });
            }
            else if (ConnectsToDatabaseType == DataStoreTypes.MySql)
            {
                dbProvider.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = "MySql.Data.MySqlClient" }
                });
            }
            else
            {
                throw new NotImplementedException();
            }
            dbProvider.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Providers"
            };
            
            var constructor = new CConstructor {ConstructorName = dbProvider.ClassName};
            constructor.Parameter.Add(new CParameter {ParameterName = "connectionString", Type = "string"});
            constructor.CodeSnippet =
                @"if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(""Connection string cannot be null or empty."", nameof(connectionString));
            }

            _connectionString = connectionString;
            ";
            dbProvider.Constructor.Add(constructor);

            var constructor2 = new CConstructor { ConstructorName = dbProvider.ClassName };
            constructor2.Parameter.AddRange(new List<CParameter>() {
                    new CParameter { Type = "string", ParameterName = "host" },
                    new CParameter { Type = "string", ParameterName = "database" },
                    new CParameter { Type = "string", ParameterName = "username" },
                    new CParameter { Type = "string", ParameterName = "password" },
                    new CParameter { Type = "int", ParameterName = "port" }
                    }
                    );

            if (_dataLayerKProject.ConnectsToDatabaseType == DataStoreTypes.Postgres)
            {
                constructor2.CodeSnippet =
              @"host = host ?? throw new ArgumentNullException(nameof(host));

                var sb = new NpgsqlConnectionStringBuilder
                {
                    Host = host,
                    Database = database,
                    Username = username,
                    Password = password,
                    Port = port
                };

                _connectionString = sb.ConnectionString;";
            }
            else if (_dataLayerKProject.ConnectsToDatabaseType == DataStoreTypes.SqlServer)
            {
                constructor2.CodeSnippet =
              @"host = host ?? throw new ArgumentNullException(nameof(host));

                var sb = new SqlConnectionStringBuilder
                {
                    DataSource = host,
                    InitialCatalog = database,
                    UserID = username,
                    Password = password
                };

                _connectionString = sb.ConnectionString;";

            }
            else
                throw new NotImplementedException();
            dbProvider.Constructor.Add(constructor2);

            dbProvider.Method.Add(new CMethod
            {
                MethodName = "GetConnection",
                ReturnType = "IDbConnection",
                CodeSnippet = codeSnippet
            });

            dbProvider.Field.Add(new CField {FieldName = "_connectionString", FieldType = "string", IsReadonly = true});
            return dbProvider;
        }

        private CClass BuildBaseDataServiceClass(CInterface dbProviderInterface )
        {
            var dataService = new CClass($"BaseDataService") { IsAbstract = true};

            dataService.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName="System" } });

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
                FieldType = $"I{_dataLayerKProject.ProjectNameAsClassNameFriendly}DbProvider",
                IsReadonly = true
            }); 

            var staticConstructor = new CConstructor
            {
                IsStatic = true,
                AccessModifier = CAccessModifier.Private,
                ConstructorName= "BaseDataService",
                CodeSnippet = @"Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;"

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
                Type = $"I{_dataLayerKProject.ProjectNameAsClassNameFriendly}DbProvider"
            });
            constructor.CodeSnippet = "DbProvider = dbProvider ?? throw new ArgumentException(nameof(dbProvider));";
            dataService.Constructor.Add(constructor);


            return dataService;
        }

        private CClass GetViewDto(CView view)
        {
            if (string.IsNullOrEmpty(view.ViewName))
                return null;

            var preConverter = new CViewToCTableConverter();
            var table = preConverter.Convert(view);

            var converter = new CTableToCClassConverter();
            var @class = converter.Convert(table, null, false);

            //overrite the default namespace logic
            @class.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.Entities"
            };
            return @class;
        }


        
        protected List<CClass> BuildTableTypeLists(IEnumerable<KTableType> tableTypes)
        {
            var listTableTypeClass = new List<CClass>();
            foreach (var tableType in tableTypes)
                listTableTypeClass.Add(BuildTableTypeList(tableType));

            return listTableTypeClass;
        }

        protected override CClass BuildTableTypeList(KTableType kTableType)
        {
            var tableTypeDto = GetTableTypeDto(kTableType.GeneratedTableType, kTableType.TableTypeName);

            var tableType = kTableType.GeneratedTableType;
            var classTableType = new CClass($"{tableType.TableName}List")
            {
                AccessModifier = CAccessModifier.Internal
            };
            classTableType.Namespace = new CNamespace
            {
                NamespaceName =
                    $"{_dataLayerKProject.CompanyName}.{_dataLayerKProject.ProjectName}{_dataLayerKProject.NamespaceSuffix}.Data.DataServices" //todo: make .Types
            };
            classTableType.Implements.Add(new CInterface { InterfaceName = "SqlMapper.ICustomQueryParameter" });

            classTableType.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Collections.Generic" }
            });

            classTableType.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Dapper" }
            });

            classTableType.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Data.SqlClient" }
            });

            classTableType.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Microsoft.SqlServer.Server" }
            });

            classTableType.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Data" }
            });
            if (_dataLayerKProject.ConnectsToDatabaseType == DataStoreTypes.SqlServer)
            {
                classTableType.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = "System.Data.SqlClient" }
                });
            }

            classTableType.NamespaceRef.Add(new CNamespaceRef(tableTypeDto.Namespace));


            // private readonly IEnumerable<int> _ids;
            //var fieldKeys = new SField { AccessModifier = SAccessModifier.Private, IsReadonly = true, FieldType = $"IEnumerable<{SqlMapper.ToClrTypeName(tableType.Column.First().ColumnType)}>", FieldName = "_keys" };
            var fieldKeys = new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = $"IEnumerable<{tableTypeDto.ClassName}>",
                FieldName = "_keys"
            };

            classTableType.Field.Add(fieldKeys);

            var constructor = new CConstructor { ConstructorName = $"{classTableType.ClassName}" };
            constructor.Parameter.Add(new CParameter { Type = $"{fieldKeys.FieldType}", ParameterName = "keys" });
            constructor.CodeSnippet = "_keys = keys;";
            classTableType.Constructor.Add(constructor);
            var methodAddParameter = new CMethod { ReturnType = "void", MethodName = "AddParameter" };
            methodAddParameter.Parameter.Add(new CParameter { Type = "IDbCommand", ParameterName = "command" });
            methodAddParameter.Parameter.Add(new CParameter { Type = "string", ParameterName = "name" });
            classTableType.Method.Add(methodAddParameter);


            var codeSnippet = new CodeWriter();
            codeSnippet.WriteLine("var sqlCommand = (SqlCommand)command;");
            codeSnippet.WriteLine("sqlCommand.CommandType = CommandType.StoredProcedure;");


            codeSnippet.WriteLine("var sqlMetaData = new[]");
            codeSnippet.WriteLine("{");
            codeSnippet.Indent();
            foreach (var column in tableType.Column)
                if (CTableTypeToSqlServerTableTypeConverter.DoesNeedLength(column.ColumnSqlDbType) &&
                    column.ColumnLength > 0)
                    codeSnippet.WriteLine(
                        $@"new SqlMetaData(""{column.ColumnName}"", SqlDbType.{column.ColumnSqlDbType}, {
                                column.ColumnLength
                            }),");
                else
                    codeSnippet.WriteLine(
                        $@"new SqlMetaData(""{column.ColumnName}"", SqlDbType.{column.ColumnSqlDbType}),");
            codeSnippet.Unindent();
            codeSnippet.WriteLine("};");


            codeSnippet.WriteLine("var value = new List<SqlDataRecord>(); ");
            codeSnippet.WriteLine("foreach (var key in _keys)");
            codeSnippet.WriteLine("{");
            codeSnippet.WriteLine("    var record = new SqlDataRecord(sqlMetaData);");
            //codeSnippet.WriteLine("    record.SetValues(key);");
            var position = 0;
            foreach (var property in tableTypeDto.Property)
            {
                codeSnippet.WriteLine($"    record.SetValue({position},key.{property.PropertyName});");
                position++;
            }
            codeSnippet.WriteLine("    value.Add(record);");
            codeSnippet.WriteLine("}");

            codeSnippet.WriteLine("var p = sqlCommand.Parameters.Add(name, SqlDbType.Structured);");
            codeSnippet.WriteLine($@"p.TypeName = ""{tableType.Schema.SchemaName}.{tableType.TableName}"";");

            codeSnippet.WriteLine("// Very Important to not set a value if there are no rows.");
            codeSnippet.WriteLine("if (value.Count > 0)");
            codeSnippet.WriteLine("{");
            codeSnippet.WriteLine("    p.Value = value;");
            codeSnippet.WriteLine("}");
            methodAddParameter.CodeSnippet = codeSnippet.ToString();

            return classTableType;
        }

    }
}