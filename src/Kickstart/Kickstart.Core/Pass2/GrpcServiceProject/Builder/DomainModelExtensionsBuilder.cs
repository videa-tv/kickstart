using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.GrpcServiceProject.Builder
{
    public class DomainModelExtensionsBuilder
    {
        public CClass BuildExtensionsClass(KGrpcProject grpcKProject, CClass domainModelClass, CProtoFile protoFile,
            string protoNamespace)
        {
            var extensionClass = new CClass($"{domainModelClass.ClassName}Extensions")
            {
                IsStatic = true,
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{grpcKProject.CompanyName}.{grpcKProject.ProjectName}{grpcKProject.NamespaceSuffix}.{grpcKProject.ProjectSuffix}.Extensions"
                }
            };

            
            return extensionClass;
        }

        public void AddToProtoMethods(CClass extensionClass, CClass modelClass, string protoNamespace)
        {
            var alias = "ProtoAlias";

            extensionClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { Alias = alias, NamespaceName = protoNamespace } });
            extensionClass.NamespaceRef.Add(
                new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Google.Protobuf" } });

            extensionClass.NamespaceRef.Add(
                new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = protoNamespace } });

            var protoMessage = modelClass?.DerivedFrom?.DerivedFrom as CProtoMessage;

            if (protoMessage == null)
                return ;
            // if (protoMessage.Rpc == null)
            //     continue;
            ;
            var toModelMethod = new CMethod
            {
                IsStatic = true,
                IsExtensionMethod = true,
                ReturnType = $"{modelClass.Namespace.NamespaceName}.{modelClass.ClassName}",
                MethodName = "ToModel"
            };

            //todo: create method for fully qualified name
            var parameterType = string.Empty;

            parameterType = $"{alias}.{protoMessage.MessageName}";
            toModelMethod.Parameter.Add(new CParameter { Type = parameterType, ParameterName = "source" });

            var codeWriter = new CodeWriter();
            codeWriter.WriteLine($"return new {modelClass.Namespace.NamespaceName}.{modelClass.ClassName}");
            codeWriter.WriteLine("{");
            codeWriter.Indent();

            var first = true;
            foreach (var property in modelClass.Property)
            {
                var protoField = FindProtoMessageField(protoMessage, property);
                if (!first)
                    codeWriter.WriteLine(",");
                first = false;


                //todo: clean this up
                if (protoField == null)
                {
                    codeWriter.Write($"//{property.PropertyName} = source.<unfound proto file>");
                }
                else if (property.Type.ToLower() == "char[]" && protoField.FieldType == GrpcType.__string)
                {
                    //assume tostring is needed

                    codeWriter.Write($"{property.PropertyName} = source.{protoField.FieldName}");
                    codeWriter.Write(".ToCharArray()");
                }
                else if (property.Type.ToLower() == "datetime" &&
                         protoField.FieldType == GrpcType.__google_protobuf_Timestamp)
                {
                    codeWriter.Write($"{property.PropertyName} = source.{protoField.FieldName}");
                    codeWriter.Write(".ToDateTime()");
                }
                else if (property.Type.ToLower() == "datetimeoffset" &&
                         protoField.FieldType == GrpcType.__google_protobuf_Timestamp)
                {
                    codeWriter.Write($"{property.PropertyName} = source.{protoField.FieldName}");
                    codeWriter.Write(".ToDateTime()");
                }
                else if (property.Type.ToLower() == "byte" && protoField.FieldType == GrpcType.__int32)
                {
                    codeWriter.Write($"{property.PropertyName} = (byte)source.{protoField.FieldName}");
                }
                else if (property.Type.ToLower() == "decimal" && protoField.FieldType == GrpcType.__string)
                {
                    codeWriter.Write($"{property.PropertyName} = Decimal.Parse(source.{protoField.FieldName})");
                }
                else if (property.Type.ToLower() == "byte[]" && protoField.FieldType == GrpcType.__bytes)
                {
                    codeWriter.Write($"{property.PropertyName} = source.{protoField.FieldName}");
                    codeWriter.Write($" != ByteString.Empty ? source.{protoField.FieldName}");
                    codeWriter.Write(".ToByteArray() : null");
                }
                else
                {
                    codeWriter.Write($"{property.PropertyName} = source.{protoField.FieldName}");
                }
            }

            codeWriter.WriteLine(string.Empty);
            codeWriter.Unindent();
            codeWriter.WriteLine("};");
            toModelMethod.CodeSnippet = codeWriter.ToString();
            extensionClass.Method.Add(toModelMethod);

        }

        private CProtoMessageField FindProtoMessageField(CProtoMessage protoMessage, CProperty property)
        {
            foreach (var protoField in protoMessage.ProtoField)
            {
                if (protoField.DerivedFrom is CColumn)
                {
                    var column = protoField.DerivedFrom as CColumn;
                    if (column.ColumnName == property.PropertyName)
                        return protoField;
                }
                if (protoField.DerivedFrom is CStoredProcedureParameter)
                {
                    var parameter = protoField.DerivedFrom as CStoredProcedureParameter;
                    if (parameter.SourceColumn.ColumnName == property.PropertyName)
                        return protoField;
                }

                if (protoField.FieldName == property.PropertyName)
                    return protoField;
            }
            return null;
        }

        public void AddToModelAsListMethods(CClass extensionsClass, CClass domainModelClass, CProtoFile protoFile, string protoNamespace)
        {
            var alias = "ProtoAlias";
            
            extensionsClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = protoNamespace } });
            extensionsClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Collections.Generic" } });
            extensionsClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Linq" } });
            extensionsClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Google.Protobuf.Collections" }
            });

            {
                if (!protoFile.ProtoMessage.Exists(m => m.MessageName == domainModelClass.ClassName))
                    return;

                var toModelMethod = new CMethod
                {
                    IsStatic = true,
                    IsExtensionMethod = true,
                    ReturnType = $"IEnumerable<{domainModelClass.Namespace.NamespaceName}.{domainModelClass.ClassName}>",
                    MethodName = "ToModel"
                };

                //todo: create method for fully qualified name
                var parameterType = string.Empty;

                parameterType = $"RepeatedField<{alias}.{domainModelClass.ClassName}>";
                toModelMethod.Parameter.Add(new CParameter { Type = parameterType, ParameterName = "source" });

                var codeWriter = new CodeWriter();
                codeWriter.WriteLine($"return source.Select(s => s.ToModel()).ToList();");
                toModelMethod.CodeSnippet = codeWriter.ToString();
                extensionsClass.Method.Add(toModelMethod);
            }
        }
    }
}
