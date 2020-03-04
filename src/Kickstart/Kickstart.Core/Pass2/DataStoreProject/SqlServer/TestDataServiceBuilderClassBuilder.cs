using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface ITestDataServiceBuilderClassBuilder
    {
        CClass BuildTestDataServiceBuilderClass(KDataStoreTestProject sqlTestKProject);
    }

    public class TestDataServiceBuilderClassBuilder : ITestDataServiceBuilderClassBuilder
    {
        public CClass BuildTestDataServiceBuilderClass(KDataStoreTestProject sqlTestKProject)
        {
            var @class = new CClass("TestDataServiceBuilder<T>")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}.DataAccess"
                },
                Implements = new List<CInterface>()
                {
                    new CInterface() { InterfaceName = "ITestDataServiceBuilder<T>" }
                },

                Where = new List<CWhere> { new CWhere() { WhereName = "T : IDbProvider" } }
            };

            AddNamespaceRefs(@class);

            @class.Field.Add(new CField { AccessModifier  = CAccessModifier.Private, IsReadonly = true, FieldType  = "TestDataService<T>", FieldName = "_dataService" });

            @class.Constructor.Add(new CConstructor()
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = "TestDataServiceBuilder",
                Parameter = new List<CParameter>() { new CParameter() { Type = "TestDataService<T>", ParameterName = "dataService" }},
                CodeSnippet = @"_dataService = dataService;"
            });
            
            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "ITestDataServiceBuilder<T>",
                MethodName = "NewSetup",
              
                CodeSnippet = @"_dataService.Reset();
                                return this;"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "ITestDataServiceBuilder<T>",
                MethodName = "WithSeedScript",
                Parameter = new List<CParameter>()
                {
                    new CParameter { Type  = "string", ParameterName = "scriptName"},
                    new CParameter { Type  = "object", ParameterName = "parameters", DefaultValue = "null"}

                },
                CodeSnippet = @" _dataService.AddSeedScript(new SqlScript(scriptName, parameters));
                                return this;"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "ITestDataServiceBuilder<T>",
                MethodName = "WithSeedQuery",
                Parameter = new List<CParameter>()
                {
                    new CParameter { Type  = "string", ParameterName = "scriptName"},
                    new CParameter { Type  = "object", ParameterName = "parameters", DefaultValue = "null"}

                },
                CodeSnippet = @" _dataService.AddSeedQuery(new SqlScript(scriptName, parameters));
                                return this;"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "ITestDataServiceBuilder<T>",
                MethodName = "WithCleanScript",
                Parameter = new List<CParameter>()
                {
                    new CParameter { Type  = "string", ParameterName = "scriptName"},
                    new CParameter { Type  = "object", ParameterName = "parameters", DefaultValue = "null"}

                },
                CodeSnippet = @" _dataService.AddCleanupScript(new SqlScript(scriptName, parameters));
                                return this;"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "TestDataService<T>",
                MethodName = "Create",
                CodeSnippet = @"return _dataService;"
            });

            return @class;
        }

        private void AddNamespaceRefs(CClass @class)
        {
            var namespaces = new List<string>
            {
                "Company.Datastore.Provider"
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
