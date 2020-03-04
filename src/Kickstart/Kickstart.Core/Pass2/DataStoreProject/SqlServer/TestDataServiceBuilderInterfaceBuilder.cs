using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface ITestDataServiceBuilderInterfaceBuilder
    {
        CInterface BuildTestDataServiceBuilderInterface(KDataStoreTestProject sqlTestKProject);
    }

    public class TestDataServiceBuilderInterfaceBuilder : ITestDataServiceBuilderInterfaceBuilder
    {
        public CInterface BuildTestDataServiceBuilderInterface(KDataStoreTestProject sqlTestKProject)
        {
            var @interface = new CInterface()
            {
                InterfaceName = "ITestDataServiceBuilder<T>",
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}.DataAccess"
                },
                
                Where = new List<CWhere> { new CWhere() { WhereName = "T : IDbProvider" } }
            };

            AddNamespaceRefs(@interface);

            
            
            @interface.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "ITestDataServiceBuilder<T>",
                MethodName = "NewSetup",
                SignatureOnly = true
            });

            @interface.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "ITestDataServiceBuilder<T>",
                MethodName = "WithSeedScript",
                Parameter = new List<CParameter>()
                {
                    new CParameter { Type  = "string", ParameterName = "scriptName"},
                    new CParameter { Type  = "object", ParameterName = "parameters", DefaultValue = "null"}

                },
                SignatureOnly = true
            });

            @interface.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "ITestDataServiceBuilder<T>",
                MethodName = "WithSeedQuery",
                Parameter = new List<CParameter>()
                {
                    new CParameter { Type  = "string", ParameterName = "scriptName"},
                    new CParameter { Type  = "object", ParameterName = "parameters", DefaultValue = "null"}

                },
                SignatureOnly = true
            });

            @interface.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "ITestDataServiceBuilder<T>",
                MethodName = "WithCleanScript",
                Parameter = new List<CParameter>()
                {
                    new CParameter { Type  = "string", ParameterName = "scriptName"},
                    new CParameter { Type  = "object", ParameterName = "parameters", DefaultValue = "null"}

                },
                SignatureOnly = true
            });

            @interface.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "TestDataService<T>",
                MethodName = "Create",
                SignatureOnly = true
            });

            return @interface;
        }

        private void AddNamespaceRefs(CInterface @interface)
        {
            var namespaces = new List<string>
            {
                "Company.Datastore.Provider" 
            };

            foreach (var ns in namespaces)
            {
                @interface.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = ns }
                });
            }
        }
    }
}
