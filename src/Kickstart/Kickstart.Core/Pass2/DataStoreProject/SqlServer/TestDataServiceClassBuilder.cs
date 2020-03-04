using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface ITestDataServiceClassBuilder
    {
        CClass BuildTestDataServiceClass(KDataStoreTestProject sqlTestKProject);
    }

    public class TestDataServiceClassBuilder : ITestDataServiceClassBuilder
    {
        public CClass BuildTestDataServiceClass(KDataStoreTestProject sqlTestKProject)
        {
            var @class = new CClass("TestDataService<T>")
            {
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}.DataAccess"
                },
                InheritsFrom = new CClass("BaseDataService<T>") { },
                Where = new List<CWhere>() { new CWhere() { WhereName = "T : IDbProvider" } }
            };
            AddNamespaceRefs(@class);

            @class.Constructor.Add(new CConstructor()
            {
                AccessModifier =  CAccessModifier.Public,
                ConstructorName = "TestDataService",
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = "IDatastoreContext<T>", ParameterName = "datastoreContext", PassToBaseClass = true}
                },
                CodeSnippet = "//EmptyConstructor"
            });
            @class.Field.Add(new CField()
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "List<ISqlScript>",
                FieldName = "_seedScripts",
                DefaultValue = "new List<ISqlScript>()"
            });
            @class.Field.Add(new CField()
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "List<ISqlScript>",
                FieldName = "_seedQueries",
                DefaultValue = "new List<ISqlScript>()"
            });
            @class.Field.Add(new CField()
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "List<ISqlScript>",
                FieldName = "_cleanScripts",
                DefaultValue = "new List<ISqlScript>()"
            });


            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "Reset",
                CodeSnippet = @"_seedScripts.Clear();
                                _seedQueries.Clear();
                                _cleanScripts.Clear();"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "AddSeedScript",
                CodeSnippet = @" _seedScripts.Add(script);",
                Parameter = new List<CParameter> { new CParameter() { Type = "ISqlScript", ParameterName = "script" } }
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "AddSeedQuery",
                CodeSnippet = @" _seedQueries.Add(script);",
                Parameter = new List<CParameter> { new CParameter() { Type = "ISqlScript", ParameterName = "script" } }
            });
            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "AddCleanupScript",
                CodeSnippet = @" _cleanScripts.Add(script);",
                Parameter = new List<CParameter> { new CParameter() { Type = "ISqlScript", ParameterName = "script" } }
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsAsync = true,
                ReturnType = "Task<T1>",
                MethodName = "Query<T1>",
                CodeSnippet = @"return await DatastoreContext.Query(query)
                                    .ConfigureAwait(false);",
                Parameter = new List<CParameter> { new CParameter() { Type = "IQuery<T1>", ParameterName = "query" } }
            });


            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsAsync = true,
                ReturnType = "Task<int>",
                MethodName = "Execute",
                CodeSnippet = @"return await DatastoreContext.Execute(command)
                                     .ConfigureAwait(false);",
                Parameter = new List<CParameter> { new CParameter() { Type = "ICommand", ParameterName = "command" } }
            });


            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsAsync = true,
                ReturnType = "Task",
                MethodName = "SeedAsync",
                CodeSnippet = @"await ExecuteAsync(_seedScripts);"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsAsync = true,
                ReturnType = "Task<IEnumerable<T1>>",
                MethodName = "SeedQueryAsync<T1>",
                CodeSnippet = @"return await QueryAsync<T1>(_seedQueries);"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsAsync = true,
                ReturnType = "Task",
                MethodName = "CleanUpAsync",
                CodeSnippet = @"await ExecuteAsync(_cleanScripts);"
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsAsync = true,
                ReturnType = "Task<IEnumerable<T1>>",
                MethodName = "QueryScriptAsync<T1>",
                CodeSnippet = @"var query = new EmbeddedSqlQuery<T1>(scriptName, parameters);
                                return await DatastoreContext.Query(query);",
                Parameter = new List<CParameter>
                {
                    new CParameter() {Type = "string", ParameterName = "scriptName"},
                    new CParameter() {Type = "object", ParameterName = "parameters", DefaultValue = "null"}
                }
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Public,
                IsAsync = true,
                ReturnType = "Task",
                MethodName = "ExecuteAsync",
                CodeSnippet = @"var command = new EmbeddedSqlCommand(scriptName, parameters);
                                await DatastoreContext.Execute(command);",
                Parameter = new List<CParameter>
                {
                    new CParameter() {Type = "string", ParameterName = "scriptName"},
                    new CParameter() {Type = "object", ParameterName = "parameters", DefaultValue = "null"}
                }
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Private,
                IsAsync = true,
                ReturnType = "Task",
                MethodName = "ExecuteAsync",
                CodeSnippet = @"foreach (var script in scripts)
                                {
                                    await DatastoreContext.Execute(script.ToCommand());
                                }",
                Parameter = new List<CParameter>
                {
                    new CParameter() {Type = "IEnumerable<ISqlScript>", ParameterName = "scripts"} 
                }
            });

            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Private,
                IsAsync = true,
                ReturnType = "Task<IEnumerable<T>>",
                MethodName = "QueryAsync<T>",
                CodeSnippet = @"var results = (await Task.WhenAll(scripts.Select(async s => await DatastoreContext.Query(s.ToQuery<T>()))))
                                    .Aggregate(Enumerable.Empty<T>(), (result, next) => result.Concat(next))
                                    .ToArray();

                                return results;",
                Parameter = new List<CParameter>
                {
                    new CParameter() {Type = "IEnumerable<ISqlScript>", ParameterName = "scripts"}
                }
            });

            return @class;
        }

        private void AddNamespaceRefs(CClass classTestInitialize)
        {
            var namespaces = new List<string>
            {
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Threading.Tasks",
                "Company.Datastore",
                "Company.Datastore.Command",
                "Company.Datastore.Query",
                "Company.Datastore.Provider"
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
