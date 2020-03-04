using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface ITestInitializationClassBuilder
    {
        CClass BuildTestInitializationClass(CProject dbTestProject , KDataStoreTestProject sqlTestKProject, CInterface dbProviderInterface);
    }

    public class TestInitializationClassBuilder : ITestInitializationClassBuilder
    {
        public CClass BuildTestInitializationClass(CProject dbTestProject , KDataStoreTestProject sqlTestKProject, CInterface dbProviderInterface)
        {
            var @class = new CClass("TestInitialization")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}"

                }
            };
            AddNamespaceRefs(@class, dbProviderInterface);
            @class.ClassAttribute.Add(new CClassAttribute() { AttributeName = "TestClass" });

            @class.Field.Add(new CField()
            {
                AccessModifier = CAccessModifier.Public,
                FieldType = "IServiceProvider",
                IsStatic = true,
                FieldName = "ServiceProvider"
            });

            @class.Field.Add(new CField()
            {
                AccessModifier = CAccessModifier.Private,
                IsConst = true,
                FieldType = "string",
                FieldName = "EnvPrefix",
                DefaultValue = @"""SAMPLE_SVC_TESTS_"""
            });

            @class.Property.Add(new CProperty()
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                Type = "string",
                PropertyName = "EnvironmentName",
                DefaultValue = @"Environment.GetEnvironmentVariable(""ASPNETCORE_ENVIRONMENT"")"
            });
            @class.Property.Add(new CProperty() { AccessModifier = CAccessModifier.Private, IsStatic = true, Type = "string", PropertyName = "ApplicationName", DefaultValue = "Environment.GetEnvironmentVariable(EnvPrefix + \"ApplicationName\") ?? \"daypart-integration-test\"" });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsStatic = true,
                ReturnType = "void",
                MethodName = "AssemblySetup",
                Attribute = new List<CMethodAttribute>() { new CMethodAttribute() { AttributeName = "AssemblyInitialize" } },
                Parameter = new List<CParameter>() { new CParameter() { Type = "TestContext", ParameterName = "testContext" } },
                CodeSnippet = "ServiceProvider = InitializeServiceProvider();"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsStatic = true,
                ReturnType = "void",
                MethodName = "AssemblyCleanup",
                Attribute = new List<CMethodAttribute>() { new CMethodAttribute() { AttributeName = "AssemblyCleanup" } },
                CodeSnippet = "ServiceProvider.Dispose();"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "TestServiceProvider",
                Attribute = new List<CMethodAttribute>() { new CMethodAttribute() { AttributeName = "TestMethod" } },
                CodeSnippet = @"var container = ((StructureMapServiceProvider)ServiceProvider).Container;
                                /*
                                 * Some of these could be intentional
                                 */
                                var dbConnectionMock = new Mock<DbConnection>();
                                var dbTransactionMock = new Mock<DbTransaction>();
                                container.Configure(c =>
                                {
                                    c.For<IDbConnection>().Use(dbConnectionMock.Object);
                                    c.For<IDbTransaction>().Use(dbTransactionMock.Object);
                                });

                                container.AssertConfigurationIsValid();"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                ReturnType = "IServiceProvider",
                MethodName = "InitializeServiceProvider",
                CodeSnippet = $@"var configuration = InitializeConfiguration();
                                var serviceCollection = InitializeServiceCollection(configuration);
                               serviceCollection.AddTransient<{dbProviderInterface.InterfaceName}>(
                                            p =>
                                            {{

                                                return new EventCaptureDbProvider(configuration.GetConnectionString(""EventCapture""));
                                            }});
                                return serviceCollection.ConfigureContainer();//configuration);"
                                });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                ReturnType = "IServiceCollection",
                MethodName = "InitializeServiceCollection",
                Parameter = new List<CParameter>() { new CParameter() { Type = "IConfigurationRoot", ParameterName = "config" } },
                CodeSnippet = @"var serviceCollection = new ServiceCollection();

                                /*ServiceCollectionExtensions.ConfigureServiceCollection(serviceCollection, config)
                                    .Configure<AppSettings>(config)
                                    .Configure<DaypartConfig>(config.GetSection(""zzzzz""));*/

                                    return serviceCollection; "
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                ReturnType = "IConfigurationRoot",
                MethodName = "InitializeConfiguration",
                CodeSnippet = @"return new ConfigurationBuilder()
                                        .AddJsonFile(""appsettings.json"", false)
                                        .AddJsonFile($""appsettings.{EnvironmentName}.json"", true)
                                            .AddEnvironmentVariables(EnvPrefix)
                                        .AddConfigCenter(ApplicationName, EnvPrefix)
                                            .Build(); "
            });

            return @class;
        }
        private void AddNamespaceRefs(CClass classTestInitialize, CInterface dbProviderInterface)
        {
            var namespaces = new List<string>
            {
                "System",
                "System.Data",
                "System.Data.Common",
                "Microsoft.Extensions.Configuration",
                "Microsoft.Extensions.DependencyInjection",
                "Microsoft.VisualStudio.TestTools.UnitTesting",
                "Moq",
                //"StructureMap",
                "Company.Configuration.ConfigCenter",

                $"{classTestInitialize.Namespace.NamespaceName}.Startup",
                $"{dbProviderInterface.Namespace.NamespaceName}"
            };

            foreach (var ns in namespaces)
            {
                classTestInitialize.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = ns }
                });
            }
        }

    }
}
