using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface IReadSqlScriptClassBuilder
    {
        CClass ReadSqlScriptClass(KDataStoreTestProject sqlTestKProject, KDataLayerProject dataLayerKProject);
    }

    public class ReadSqlScriptClassBuilder : IReadSqlScriptClassBuilder
    {
        public CClass ReadSqlScriptClass(KDataStoreTestProject sqlTestKProject, KDataLayerProject dataLayerKProject)
        {
            var @class = new CClass("ReadSqlScript")
            {
                IsStatic = true,
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}.DataAccess"
                } 
            };

            AddNamespaceRefs(sqlTestKProject, @class);
   
            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Internal,
                IsStatic = true,
                ReturnType = "string",
                MethodName = "FromEmbeddedResource",
                Parameter = new List<CParameter>() { new CParameter() { Type = "string", ParameterName = "scriptName" } },
                CodeSnippet = $@"var sourceName = $""{dataLayerKProject.ProjectFullName}.EmbeddedSql.{{scriptName}}.esql"";
                                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("".Data.SqlServer""));
                                var dataAssembly = assemblies.First();
                                return dataAssembly.ReadToEndEmbeddedResource(sourceName);"
            });
          
            return @class;
        }

        private void AddNamespaceRefs(KDataStoreTestProject sqlTestKProject,CClass @class)
        {
            var namespaces = new List<string>
            {
                "System",
                "System.Linq",
                $"{sqlTestKProject.ProjectFullName}.Extensions"
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
