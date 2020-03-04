using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface IEmbeddedSqlCommandBuilder
    {
        CClass BuildEmbeddedSqlCommandBuilderClass(KDataStoreTestProject sqlTestKProject);
    }

    public class EmbeddedSqlCommandBuilder : IEmbeddedSqlCommandBuilder
    {
        public CClass BuildEmbeddedSqlCommandBuilderClass(KDataStoreTestProject sqlTestKProject)
        {
            var @class = new CClass("EmbeddedSqlCommand")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}.DataAccess"
                },
                Implements = new List<CInterface>() { new CInterface() { InterfaceName = "ICommand" } }
            };

            AddNamespaceRefs(@class);

            @class.Constructor.Add(new CConstructor()
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = "EmbeddedSqlCommand",
                Parameter = new List<CParameter>() {
                    new CParameter() { Type = "string", ParameterName = "scriptName" },
                    new CParameter() { Type = "object", ParameterName = "param", DefaultValue = "null"},
                    new CParameter() { Type = "int?", ParameterName = "commandTimeout", DefaultValue = "null"},
                    new CParameter() { Type = "CommandType?", ParameterName = "commandType", DefaultValue = "null"},

                },
                CodeSnippet = @"_query = ReadSqlScript.FromEmbeddedResource(scriptName);
                                _parameters = param;
                                _commandTimeout = commandTimeout;
                                _commandType = commandType;"
            });

            @class.Field.Add(new CField() { IsReadonly = true, FieldType = "string", FieldName = "_query" });
            @class.Field.Add(new CField() { IsReadonly = true, FieldType = "object", FieldName = "_parameters" });
            @class.Field.Add(new CField() { IsReadonly = true, FieldType = "int?", FieldName = "_commandTimeout" });
            @class.Field.Add(new CField() { IsReadonly = true, FieldType = "CommandType?", FieldName = "_commandType" });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsAsync = true,
                ReturnType = "Task<int>",
                MethodName = "Execute",
                Parameter = new List<CParameter> { new CParameter() { Type = "IDatastoreConnectionProxy", ParameterName = "datastoreConnectionProxy" } },
                CodeSnippet = @"return await datastoreConnectionProxy.Execute(_query, _parameters, _commandTimeout, _commandType)
                                        .ConfigureAwait(false); "
            });
           
            return @class;
        }

        private void AddNamespaceRefs(CClass @class)
        {
            var namespaces = new List<string>
            {
                "System.Collections.Generic",
                "System.Data",
                "System.Threading.Tasks",
                "Company.Datastore",
                "Company.Datastore.Query",
                "Company.Datastore.Command",
                "Company.Datastore.Connection"
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
