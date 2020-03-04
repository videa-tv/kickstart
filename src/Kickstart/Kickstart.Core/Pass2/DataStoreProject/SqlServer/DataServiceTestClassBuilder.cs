using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.DataLayerProject;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface IDataServiceTestClassBuilder
    {
        CClass BuildDataServiceTestClass(KDataStoreProject sqlKProject, KDataLayerProject dataLayerKProject, KDataStoreTestProject sqlTestKProject, CInterface dbProviderInterface);
    }

    public class DataServiceTestClassBuilder : IDataServiceTestClassBuilder
    {
        public CClass BuildDataServiceTestClass(KDataStoreProject sqlKProject, KDataLayerProject dataLayerKProject, KDataStoreTestProject sqlTestKProject, CInterface dbProviderInterface)
        {
            var @class = new CClass("EventCaptureDataServiceTest")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}"
                },
                ClassAttribute = new List<CClassAttribute>() { new CClassAttribute() { AttributeName = "TestClass" } }
            };

            AddNamespaceRefs(@class, dbProviderInterface);

            @class.Field.Add(new CField()
            {
                AccessModifier = CAccessModifier.Private,
                IsStatic = true,
                FieldType = $"TestDataService<{dbProviderInterface.InterfaceName}>",
                FieldName = "_testDataService"
            });
            
            @class.Method.Add(new CMethod()
            {   
                Attribute =  new List<CMethodAttribute>() { new CMethodAttribute() { AttributeName = "ClassInitialize" } },
                AccessModifier = CAccessModifier.Public,
                IsStatic = true,
                ReturnType = "void",
                MethodName = "ClassInitialize",
                Parameter = new List<CParameter>() { new CParameter() { Type = "TestContext", ParameterName = "context" } },
                CodeSnippet = $@"var testDataServiceBuilder = 
                                     TestInitialization.ServiceProvider.GetService<ITestDataServiceBuilder<{dbProviderInterface.InterfaceName}>>();

                                _testDataService = testDataServiceBuilder.Create();

                                _testDataService.SeedAsync()
                                    .GetAwaiter()
                                    .GetResult();"
            });

            var service = new DataLayerServiceProjectServiceBase();
            foreach (var kStoredProcedure in sqlKProject.StoredProcedure)
            {
                if (!string.IsNullOrEmpty(kStoredProcedure.ParameterSetName))
                {
                    var @entityClass = service.BuildParameterEntityClass(kStoredProcedure.GeneratedStoredProcedure, kStoredProcedure.ParameterSetName);
                    int x = 1;
                }

                if (!string.IsNullOrWhiteSpace(kStoredProcedure.ResultSetName))
                {
                    var @entityClass = service.BuildResultEntityClass(kStoredProcedure.GeneratedStoredProcedure, kStoredProcedure.ResultSetName, dataLayerKProject);
                    @class.Method.Add(new CMethod()
                    {
                        Attribute = new List<CMethodAttribute>() { new CMethodAttribute() { AttributeName = "TestMethod" } },

                        AccessModifier = CAccessModifier.Public,
                        IsAsync = true,
                        ReturnType = "Task",
                        MethodName = $"Test{kStoredProcedure.StoredProcedureName}",
                        CodeSnippet = $@"var result = await _testDataService.Query(new EmbeddedSqlQuery<{@entityClass.ClassName}>(""{kStoredProcedure.StoredProcedureName}""));"
                    });
                }
                
            }


          

            return @class;
        }

        private void AddNamespaceRefs(CClass @class, CInterface dbProviderInterface)
        {
            var namespaces = new List<string>
            {
                "System.Collections.Generic",
                "System.Threading.Tasks",
                "Microsoft.Extensions.DependencyInjection",
                "Microsoft.VisualStudio.TestTools.UnitTesting",
                "Company.Datastore",
                "Company.Datastore.Query",
                "Company.Datastore.Command",
                $"{dbProviderInterface.Namespace.NamespaceName}",
                $"{@class.Namespace.NamespaceName}.DataAccess"

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
