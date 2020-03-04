using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;
using Newtonsoft.Json;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface ISqlServerTestProjectService : IDataStoreTestProjectService
    {
    }

    public class SqlServerTestProjectService : ISqlServerTestProjectService
    {
        private KDataStoreProject _dataStoreKProject;
        private KDataStoreTestProject _sqlTestKProject;
        private KDataLayerProject _dataLayerKProject;
        private CProject _dataStoreProject;
        private CInterface _dbProviderInterface;
        private CClass _dbProviderClass;
        private readonly ITestDataServiceClassBuilder _testDataServiceClassBuilder;
        private readonly ITestInitializationClassBuilder _testInitializationClassBuilder;
        private readonly IStartupContainerClassBuilder _startupContainerClassBuilder;
        private readonly ISqlScriptClassBuilder _sqlScriptClassBuilder;
        private readonly ISqlScriptInterfaceBuilder _sqlScriptInterfaceBuilder;
        private readonly IEmbeddedSqlCommandBuilder _embeddedSqlCommandBuilder;
        private readonly IEmbeddedSqlQueryBuilder _embeddedSqlQueryBuilder;
        private readonly IReadSqlScriptClassBuilder _readSqlScriptClassBuilder;
        private readonly IAssemblyExtensionsClassBuilder _assemblyExtensionsClassBuilder;
        private readonly IDataServiceTestClassBuilder _dataServiceTestClassBuilder;
        private readonly ITestDataServiceBuilderClassBuilder _testDataServiceBuilderClassBuilder;
        private readonly ITestDataServiceBuilderInterfaceBuilder _testDataServiceBuilderInterfaceBuilder;

        public SqlServerTestProjectService(ITestDataServiceClassBuilder testDataServiceClassBuilder,
            ITestInitializationClassBuilder testInitializationClassBuilder,
            IStartupContainerClassBuilder startupContainerClassBuilder, ISqlScriptClassBuilder sqlScriptClassBuilder,
            ISqlScriptInterfaceBuilder sqlScriptInterfaceBuilder, 
            IEmbeddedSqlCommandBuilder embeddedSqlCommandBuilder,
            IEmbeddedSqlQueryBuilder embeddedSqlQueryBuilder, 
            IReadSqlScriptClassBuilder readSqlScriptClassBuilder,
            IAssemblyExtensionsClassBuilder assemblyExtensionsClassBuilder, IDataServiceTestClassBuilder dataServiceTestClassBuilder, ITestDataServiceBuilderClassBuilder testDataServiceBuilderClassBuilder, ITestDataServiceBuilderInterfaceBuilder testDataServiceBuilderInterfaceBuilder)
        {
            _testDataServiceClassBuilder = testDataServiceClassBuilder;
            _testInitializationClassBuilder = testInitializationClassBuilder;
            _startupContainerClassBuilder = startupContainerClassBuilder;
            _sqlScriptClassBuilder = sqlScriptClassBuilder;
            _sqlScriptInterfaceBuilder = sqlScriptInterfaceBuilder;
            _embeddedSqlCommandBuilder = embeddedSqlCommandBuilder;
            _embeddedSqlQueryBuilder = embeddedSqlQueryBuilder;
            _readSqlScriptClassBuilder = readSqlScriptClassBuilder;
            _assemblyExtensionsClassBuilder = assemblyExtensionsClassBuilder;
            _dataServiceTestClassBuilder = dataServiceTestClassBuilder;
            _testDataServiceBuilderClassBuilder = testDataServiceBuilderClassBuilder;
            _testDataServiceBuilderInterfaceBuilder = testDataServiceBuilderInterfaceBuilder;
        }

        public CProject BuildProject(KSolution kSolution, KDataStoreProject dataStoreKProject, KDataStoreTestProject sqlTestKProject,
            KDataLayerProject dataLayerKProject,
            CProject dataStoreProject, CInterface dbProviderInterface, CClass dbProviderClass)
        {
            _dataStoreKProject = dataStoreKProject;
            _sqlTestKProject = sqlTestKProject;
            _dataLayerKProject = dataLayerKProject;
            _dataStoreProject = dataStoreProject;
            _dbProviderInterface = dbProviderInterface;
            _dbProviderClass = dbProviderClass;

            var dbTestProject = new CProject
            {
                ProjectName = sqlTestKProject.ProjectFullName, 
                ProjectShortName = sqlTestKProject.ProjectName, 
                ProjectFolder = sqlTestKProject.ProjectFolder,
                ProjectType = CProjectType.CsProj
            };

            dbTestProject.ProjectReference.Add(dataStoreProject);

            dbTestProject.TemplateProjectPath = @"templates\NetCore31ConsoleApp.csproj";
            
            AddTestInitializationClass(dbTestProject);
            AddStartupContainerClass(dbTestProject);
            AddTestDataServiceClass(dbTestProject);
            AddTestDataServiceBuilderInterface(dbTestProject);
            AddTestDataServiceBuilderClass(dbTestProject);
            AddReadSqlScriptClass(dbTestProject);
            AddSqlScriptInterface(dbTestProject);
            AddSqlScriptClass(dbTestProject);
            AddEmbeddedSqlCommandClass(dbTestProject);
            AddEmbeddedSqlQueryClass(dbTestProject);
            AddAssemblyExtensionClass(dbTestProject);
            AddAppSettingsJson(dbTestProject);
            AddDataServiceTestClass(dbTestProject);
            AddNuGetRefs(dbTestProject);

            return dbTestProject;
        }

        private void AddTestDataServiceBuilderInterface(CProject dbTestProject)
        {
            var @interface = _testDataServiceBuilderInterfaceBuilder.BuildTestDataServiceBuilderInterface(_sqlTestKProject);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @interface,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"DataAccess", FileName = $"{@interface.InterfaceName.Replace("<T>","")}.cs" }
            });
        }

        private void AddTestDataServiceBuilderClass(CProject dbTestProject)
        {
            var @class = _testDataServiceBuilderClassBuilder.BuildTestDataServiceBuilderClass(_sqlTestKProject);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"DataAccess", FileName = $"{@class.ClassName.Replace("<T>","")}.cs" }
            });
        }

        private void AddDataServiceTestClass(CProject dbTestProject)
        {
            var @class = _dataServiceTestClassBuilder.BuildDataServiceTestClass(_dataStoreKProject,_dataLayerKProject,  _sqlTestKProject, _dbProviderInterface);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"", FileName = $"{@class.ClassName}.cs" }
            });
        }

        private void AddAssemblyExtensionClass(CProject dbTestProject)
        {
            var @class = _assemblyExtensionsClassBuilder.BuildAssemblyExtensionsClass(_sqlTestKProject);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Extensions", FileName = $"{@class.ClassName}.cs" }
            });
        }

        private void AddEmbeddedSqlQueryClass(CProject dbTestProject)
        {
            var @class =
                _embeddedSqlQueryBuilder.BuildEmbeddedSqlQueryBuilderClass(_sqlTestKProject);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"DataAccess", FileName = $"{@class.ClassName.Replace("<T>","")}.cs" }
            });

        }
        private void AddEmbeddedSqlCommandClass(CProject dbTestProject)
        {
            var @class =
                _embeddedSqlCommandBuilder.BuildEmbeddedSqlCommandBuilderClass(_sqlTestKProject);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"DataAccess", FileName = $"{@class.ClassName.Replace("<T>","")}.cs" }
            });

        }
        private void AddReadSqlScriptClass(CProject dbTestProject)
        {
            var @class =
                _readSqlScriptClassBuilder.ReadSqlScriptClass(_sqlTestKProject, _dataLayerKProject);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"DataAccess", FileName = $"{@class.ClassName}.cs" }
            });

        }
        private void AddSqlScriptClass(CProject dbTestProject)
        {
            var @class =
                _sqlScriptClassBuilder.BuildSqlScriptClass(_sqlTestKProject);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"DataAccess", FileName = $"{@class.ClassName}.cs" }
            });

        }

        private void AddSqlScriptInterface(CProject dbTestProject)
        {
            var @interface =
                _sqlScriptInterfaceBuilder.BuildSqlScriptInterface(_sqlTestKProject);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @interface,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"DataAccess", FileName = $"{@interface.InterfaceName}.cs" }
            });
        }


        private void AddTestInitializationClass(CProject dbTestProject)
        {
            var @class =
                _testInitializationClassBuilder.BuildTestInitializationClass(dbTestProject, _sqlTestKProject, _dbProviderInterface);

            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"", FileName = $"{@class.ClassName}.cs" }
            });
        }

        private void AddTestDataServiceClass(CProject dbTestProject)
        {
            var @class = _testDataServiceClassBuilder.BuildTestDataServiceClass(_sqlTestKProject);
            
            dbTestProject.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"DataAccess", FileName = $"{@class.ClassName.Replace("<T>","")}.cs" }
            });
        }

        

        
        
        private static void AddNuGetRefs(CProject dbTestProject)
        {
            dbTestProject.NuGetReference.Add(new CNuGet {NuGetName = "System.Data.SqlClient", Version = "4.4.0"});
            dbTestProject.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Configuration.Binder",
                Version = "2.1.0"
            });
            dbTestProject.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Configuration.EnvironmentVariables",
                Version = "2.1.0"
            });
            dbTestProject.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Configuration.Json",
                Version = "2.1.0"
            });
            dbTestProject.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.DependencyInjection",
                Version = "2.1.0"
            });
            dbTestProject.NuGetReference.Add(new CNuGet {NuGetName = "Microsoft.Extensions.Logging", Version = "2.1.0"});
            dbTestProject.NuGetReference.Add(new CNuGet
            {
                NuGetName = "Microsoft.Extensions.Options.ConfigurationExtensions",
                Version = "2.1.0"
            });
            dbTestProject.NuGetReference.Add(new CNuGet {NuGetName = "Microsoft.NET.Test.Sdk", Version = "15.8.0"});
            dbTestProject.NuGetReference.Add(new CNuGet {NuGetName = "Moq", Version = "4.8.3"});
            dbTestProject.NuGetReference.Add(new CNuGet {NuGetName = "MSTest.TestAdapter", Version = "1.3.2"});
            dbTestProject.NuGetReference.Add(new CNuGet {NuGetName = "MSTest.TestFramework", Version = "1.3.2"});
            dbTestProject.NuGetReference.Add(new CNuGet {NuGetName = "StructureMap", Version = "4.7.0"});
            dbTestProject.NuGetReference.Add(new CNuGet
            {
                NuGetName = "StructureMap.Microsoft.DependencyInjection",
                Version = "1.4.0"
            });
            dbTestProject.NuGetReference.Add(new CNuGet {NuGetName = "Company.Configuration.ConfigCenter", Version = "1.1.0"});
            dbTestProject.NuGetReference.Add(new CNuGet { NuGetName = "Company.Datastore", Version = "2.0.0" });
            dbTestProject.NuGetReference.Add(new CNuGet { NuGetName = "Company.Datastore.Dapper", Version = "2.0.0" });

        }

        protected void AddStartupContainerClass(CProject project)
        {
            var @class = _startupContainerClassBuilder.BuildStartupContainerClass(_sqlTestKProject, _dbProviderInterface, _dbProviderClass);

            project.ProjectContent.Add(new CProjectContent
            {
                Content = @class,
                BuildAction = /*_sqlTestKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile :*/ CBuildAction.DoNotInclude,
                File = new CFile { Folder = $@"Startup", FileName = $"Startup.Container.cs" }
            });
        }

        protected void AddAppSettingsJson(CProject project)
        {
            
            var jsonSnippet = new CodeWriter();
            jsonSnippet.WriteLine("{");
            
            jsonSnippet.WriteLine($@"
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
            if (_sqlTestKProject.DataStoreType == DataStoreTypes.MySql)
            {
                connectionString = $"Data Source=localhost;Initial Catalog={_dataStoreProject.ProjectShortName};Uid=root;Pwd=my-secret-pw;";
            }
            else if (_sqlTestKProject.DataStoreType == DataStoreTypes.Postgres)
            {
                connectionString = $"Server=localhost;Port=5432; Database={_dataStoreProject.ProjectShortName.ToLower()};User Id=postgres;Password=my-secret-pw;";
            }


            if (_dataStoreProject != null)
            {
                jsonSnippet.WriteLine($@",
                                        ""ConnectionStrings"": {{
                                        ""{_sqlTestKProject.ProjectName}"": ""{connectionString} ""
                                        }}");

            }

            
            if (_sqlTestKProject.DataStoreType == DataStoreTypes.Postgres)//TODO: should do for all dbms, only port should be unique
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

            if (false) //TODO:
            {
                jsonSnippet.WriteLine(
                                         $@",
                                        ""Authentication"": {{
                                        ""Enabled"": false,
                                        ""ApiKey"" : """"
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

    }
}
