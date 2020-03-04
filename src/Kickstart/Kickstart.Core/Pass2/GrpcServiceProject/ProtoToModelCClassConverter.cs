using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.GrpcServiceProject
{
    internal class ProtoToModelCClassConverter
    {
        public void AddProtoToModelMethods(CClass extensionsClass, CProtoFile protoFile, CClass convertFromProtoClass,
            string protoNamespace)
        {
            var alias = "ProtoAlias";

            extensionsClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Collections.Generic" } });

            extensionsClass.NamespaceRef.Add(new CNamespaceRef {ReferenceTo = new CNamespace {NamespaceName = protoNamespace}});
            //todo: should we use SProtoFile instead?
            //foreach (var convertFromProtoClass in convertFromProtoClasses)
            {
                var protoMessage = protoFile.ProtoMessage.FirstOrDefault(m => m.MessageName == convertFromProtoClass.ClassName);

                if (protoMessage == null)
                    return;

                var toProtoMethod = new CMethod
                {
                    IsStatic = true,
                    IsExtensionMethod = true,
                    ReturnType = convertFromProtoClass.ClassName,
                    MethodName = "ToModel"
                };

                //todo: create method for fully qualified name
                toProtoMethod.Parameter.Add(new CParameter
                {
                    Type = $"{convertFromProtoClass.Namespace.NamespaceName}.{convertFromProtoClass.ClassName}",
                    ParameterName = "source"
                });

                var codeWriter = new CodeWriter();
                codeWriter.WriteLine($"return new {convertFromProtoClass.ClassName}");
                codeWriter.WriteLine("{");
                codeWriter.Indent();

                var first = true;
                foreach (var property in convertFromProtoClass.Property)
                {
                    //var protoField = protoMessage.ProtoField.FirstOrDefault(pf => pf.FieldName == property.PropertyName.Replace("@", ""));

                    var protoField = FindProtoMessageField(protoMessage, property);
                    if (!first)
                        codeWriter.WriteLine(",");
                    first = false;

                    if (protoField == null)
                    {
                        codeWriter.Write($"//<unknownProtoField> = source.{property.PropertyName}");
                    }
                    else if (property.Type.ToLower() == "char[]" && protoField.FieldType == GrpcType.__string)
                    {
                        codeWriter.Write($"{protoField.FieldName} = source.{property.PropertyName}");

                        //assume tostring is needed
                        codeWriter.Write(".ToString()");
                    }
                    else if (property.Type.ToLower() == "decimal" && protoField.FieldType == GrpcType.__string)
                    {
                        codeWriter.Write($"{protoField.FieldName} = source.{property.PropertyName}");

                        //assume tostring is needed
                        codeWriter.Write(".ToString()");
                    }
                    else if (property.Type.ToLower() == "byte" && protoField.FieldType == GrpcType.__int32)
                    {
                        codeWriter.Write($"{protoField.FieldName} = (int) source.{property.PropertyName}");
                    }
                    else if (property.Type.ToLower() == "datetime" &&
                             protoField.FieldType == GrpcType.__google_protobuf_Timestamp)
                    {
                        codeWriter.Write(
                            $"{protoField.FieldName} = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(source.{property.PropertyName}, DateTimeKind.Utc))");
                    }
                    else if (property.Type.ToLower() == "datetimeoffset" &&
                             protoField.FieldType == GrpcType.__google_protobuf_Timestamp)
                    {
                        codeWriter.Write(
                            $"{protoField.FieldName} = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(source.{property.PropertyName}.DateTime, DateTimeKind.Utc))");
                    }
                    else if (property.Type.ToLower() == "byte[]" && protoField.FieldType == GrpcType.__bytes)
                    {
                        codeWriter.Write(
                            $"{protoField.FieldName} = Google.Protobuf.ByteString.CopyFrom(source.{property.PropertyName})");
                    }

                    else
                    {
                        codeWriter.Write($"{protoField.FieldName} = source.{property.PropertyName}");
                    }
                }
                codeWriter.WriteLine(string.Empty);
                codeWriter.Unindent();
                codeWriter.WriteLine("};");
                toProtoMethod.CodeSnippet = codeWriter.ToString();
                extensionsClass.Method.Add(toProtoMethod);
            }
            
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
            }
            return null;
        }
    }
}