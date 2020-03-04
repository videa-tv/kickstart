using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface IAssemblyExtensionsClassBuilder
    {
        CClass BuildAssemblyExtensionsClass(KDataStoreTestProject sqlTestKProject);
    }

    public class AssemblyExtensionsClassBuilder : IAssemblyExtensionsClassBuilder
    {
        public CClass BuildAssemblyExtensionsClass(KDataStoreTestProject sqlTestKProject)
        {
            var @class = new CClass("AssemblyExtensions")
            {
                AccessModifier =  CAccessModifier.Internal,
                IsStatic = true,
                Namespace = new CNamespace()
                {
                    NamespaceName =
                        $"{sqlTestKProject.ProjectFullName}.Extensions"
                } 
            };

            AddNamespaceRefs(@class);
   
            @class.Method.Add(new CMethod()
            {
                AccessModifier = CAccessModifier.Internal,
                IsStatic = true,
                IsExtensionMethod = true,
                ReturnType = "string",
                MethodName = "ReadToEndEmbeddedResource",
                Parameter = new List<CParameter>()
                {
                    new CParameter() { Type = "Assembly", ParameterName = "assembly" },
                    new CParameter() { Type = "string", ParameterName = "resourceName"}
                },
                CodeSnippet = $@"if (assembly == null)
                                {{
                                    throw new ArgumentNullException(nameof(assembly));
                                }}

                                var stream = assembly.GetManifestResourceStream(resourceName);
                                if (stream == null)
                                {{
                                    throw new InvalidOperationException($""Embedded resource {{resourceName}} was not found in assembly <{{assembly.FullName}}>"");
                                }}

                                using (stream)
                                using (var reader = new StreamReader(stream))
                                {{
                                    return reader.ReadToEnd();
                                }}"
            });
          
            return @class;
        }

        private void AddNamespaceRefs(CClass @class)
        {
            var namespaces = new List<string>
            {
                "System",
                "System.IO",
                "System.Reflection"
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
