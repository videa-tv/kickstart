using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface ISqlScriptInterfaceBuilder
    {
        CInterface BuildSqlScriptInterface(KDataStoreTestProject sqlTestKProject);
    }

    public class SqlScriptInterfaceBuilder : ISqlScriptInterfaceBuilder
    {
        public CInterface BuildSqlScriptInterface(KDataStoreTestProject sqlTestKProject)
        {
            var @interface = new CInterface()
            {
                InterfaceName = "ISqlScript",
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}.DataAccess"
                },
              
            };

            AddNamespaceRefs(@interface);

            @interface.Method.Add(new CMethod() { SignatureOnly = true,   ReturnType = "ICommand", MethodName = "ToCommand"});
            @interface.Method.Add(new CMethod() { SignatureOnly = true, ReturnType = "IQuery<IEnumerable<T>>", MethodName = "ToQuery<T>"});

            return @interface;
        }

        private void AddNamespaceRefs(CInterface @interface)
        {
            var namespaces = new List<string>
            {
                "System.Collections.Generic", 
                "Company.Datastore",
                "Company.Datastore.Query",
                "Company.Datastore.Command"
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
