using System;
using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface IModelToProtoCClassConverter
    {
        void AddModelToProtoMethods(CClass extensionsClass, CProtoFile protoFile, CClass modelClass,
            string protoNamespace);
    }

    internal class ModelToProtoCClassConverter : IModelToProtoCClassConverter
    {
        public void AddModelToProtoMethods(CClass extensionsClass, CProtoFile protoFile, CClass modelClass,
            string protoNamespace)
        {
            var alias = "ProtoAlias";
            
            extensionsClass.NamespaceRef.Add(new CNamespaceRef("System.Collections.Generic"));
            extensionsClass.NamespaceRef.Add(new CNamespaceRef("System.Linq"));

            extensionsClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { Alias = alias, NamespaceName = protoNamespace}});

            foreach (var protoMessageIn in protoFile.ProtoMessage)
            {

                CClass convertFromModelClass = null;
                //foreach (var modelClass in convertFromModelClasses)
                {
                    var pm = modelClass?.DerivedFrom?.DerivedFrom as CProtoMessage;
                    if (pm != null && pm.MessageName == protoMessageIn.MessageName)
                    {
                        convertFromModelClass = modelClass;
                        break;
                    }
                }
                if (convertFromModelClass == null)
                    return;

                var protoMessage = protoMessageIn; //protoFile.ProtoMessage.FirstOrDefault(m => m.MessageName == convertFromClass.ClassName);
               // var protoMessage2 = convertFromModelClass?.DerivedFrom?.DerivedFrom as CProtoMessage;
                //if (protoMessage2 == null)
                //    continue;

                if (protoMessage.ProtoField.Count == 1 &&
                    protoMessage.ProtoField.First().FieldType == GrpcType.__message)
                {
                    protoMessage = protoMessage.Rpc.ProtoService.ProtoFile.GetRepeatedMessagesUsedInAResponse()
                        .FirstOrDefault(m => m.MessageName == protoMessage.ProtoField.First().MessageType);
                }

                if (protoMessage == null)
                    return;

                var toProtoMethod = new CMethod
                {
                    IsStatic = true,
                    IsExtensionMethod = true,
                    ReturnType = $"{alias}.{protoMessage.MessageName}",
                    MethodName = "ToProto"
                };

                //todo: create method for fully qualified name
                toProtoMethod.Parameter.Add(new CParameter
                {
                    Type = $"{convertFromModelClass.Namespace.NamespaceName}.{convertFromModelClass.ClassName}",
                    ParameterName = "source"
                });

                var codeWriter = new CodeWriter();
                codeWriter.WriteLine($"return new {alias}.{protoMessage.MessageName}");
                codeWriter.WriteLine("{");
                codeWriter.Indent();

                var first = true;
                foreach (var property in convertFromModelClass.Property)
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
                    else if (property.Type.ToLower() == "decimal" &&
                             protoField.FieldType == GrpcType.__company_Decimal64Value)
                    {
                        codeWriter.Write($"{protoField.FieldName} = new Decimal64Value() ");
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


                var toProtoForListMethod = new CMethod
                {
                    IsStatic = true,
                    IsExtensionMethod = true,
                    ReturnType = $"IEnumerable<{alias}.{protoMessage.MessageName}>",
                    MethodName = "ToProto"
                };

                //todo: create method for fully qualified name
                var parameterType = string.Empty;

                parameterType =
                    $"IEnumerable<{convertFromModelClass.Namespace.NamespaceName}.{convertFromModelClass.ClassName}>";
                toProtoForListMethod.Parameter.Add(new CParameter {Type = parameterType, ParameterName = "source"});

                var codeWriter3 = new CodeWriter();
                codeWriter3.WriteLine($"return source.Select(s => s.ToProto()).ToList();");
                toProtoForListMethod.CodeSnippet = codeWriter3.ToString();
                extensionsClass.Method.Add(toProtoForListMethod);
                
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

                if (protoField.FieldName == property.PropertyName)
                {
                    return protoField;
                }
            }
            return null;
        }
    }
}