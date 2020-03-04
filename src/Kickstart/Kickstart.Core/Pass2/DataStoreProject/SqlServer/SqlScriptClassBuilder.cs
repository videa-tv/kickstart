using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface ISqlScriptClassBuilder
    {
        CClass BuildSqlScriptClass(KDataStoreTestProject sqlTestKProject);
    }

    public class SqlScriptClassBuilder : ISqlScriptClassBuilder
    {
        public CClass BuildSqlScriptClass(KDataStoreTestProject sqlTestKProject)
        {
            var @class = new CClass("SqlScript")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}.DataAccess"
                },
                Implements = new List<CInterface>() { new CInterface() { InterfaceName = "ISqlScript" } }
            };

            AddNamespaceRefs(@class);

            @class.Constructor.Add(new CConstructor()
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = "SqlScript",
                Parameter = new List<CParameter>() { new CParameter() { Type = "string", ParameterName = "name" }, new CParameter() { Type = "object", ParameterName = "parameters", DefaultValue = "null"} },
                CodeSnippet = @"Name = name;
                                Parameters = parameters; "
            });

            @class.Property.Add(new CProperty() { Type = "string", PropertyName = "Name"});
            @class.Property.Add(new CProperty() { Type = "object", PropertyName = "Parameters" });
            
            @class.Method.Add(new CMethod() { AccessModifier = CAccessModifier.Public, ReturnType = "ICommand", MethodName = "ToCommand", UseExpressionDefinition = true, CodeSnippet = "new EmbeddedSqlCommand(Name, Parameters);" });
            @class.Method.Add(new CMethod() { AccessModifier = CAccessModifier.Public, ReturnType = "IQuery<IEnumerable<T>>", MethodName = "ToQuery<T>", UseExpressionDefinition = true, CodeSnippet = "new EmbeddedSqlQuery<T>(Name, Parameters);" });

            return @class;
        }

        private void AddNamespaceRefs(CClass @class)
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
                @class.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace { NamespaceName = ns }
                });
            }
        }
    }
}
