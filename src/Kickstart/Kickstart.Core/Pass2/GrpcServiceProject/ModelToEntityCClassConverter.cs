using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface IModelToEntityCClassConverter
    {
        void AddModelToEntityMethods(CClass extensionsClass, CClass convertFromClass,
            CProject dataLayerProject,
            string namespaceName);
    }

    public class ModelToEntityCClassConverter : IModelToEntityCClassConverter
    {
        public void AddModelToEntityMethods(CClass extensionsClass, CClass convertFromModelClass, CProject dataLayerProject,
            string namespaceName)
        {
            var alias = "EntityAlias";
            
            //todo: should we use SProtoFile instead?
            //foreach (var convertFromModelClass in convertFromModelClasses)
            {
                //don't have a direct connection. See if they have a common Table
                var entityClass =
                    dataLayerProject.Class.FirstOrDefault(c => c.DerivedFrom == convertFromModelClass.DerivedFrom);

                if (entityClass == null)
                    return;

                extensionsClass.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace {Alias = alias, NamespaceName = entityClass.Namespace.NamespaceName}
                });

                var toEntityMethod = new CMethod
                {
                    IsStatic = true,
                    IsExtensionMethod = true,
                    ReturnType = $"{alias}.{entityClass.ClassName}",
                    MethodName = "ToEntity"
                };

                //todo: create method for fully qualified name
                toEntityMethod.Parameter.Add(new CParameter
                {
                    Type = $"{convertFromModelClass.Namespace.NamespaceName}.{convertFromModelClass.ClassName}",
                    ParameterName = "source"
                });

                var codeWriter = new CodeWriter();
                codeWriter.WriteLine($"return new {alias}.{entityClass.ClassName}");
                codeWriter.WriteLine("{");
                codeWriter.Indent();

                var first = true;
                foreach (var modelProperty in convertFromModelClass.Property)
                {

                    var entityProperty =
                        entityClass.Property.FirstOrDefault(p => p.PropertyName == modelProperty.PropertyName); //  FindProtoMessageField(protoMessage, property);
                    if (!first)
                        codeWriter.WriteLine(",");
                    first = false;

                    if (entityProperty == null)
                    {
                        codeWriter.Write($"//<unknownEntityProperty> = source.{modelProperty.PropertyName}");
                    }
                    
                    else
                    {
                        codeWriter.Write($"{entityProperty.PropertyName} = source.{modelProperty.PropertyName}");
                    }
                }
                codeWriter.WriteLine(string.Empty);
                codeWriter.Unindent();
                codeWriter.WriteLine("};");
                toEntityMethod.CodeSnippet = codeWriter.ToString();
                extensionsClass.Method.Add(toEntityMethod);


                var ToEntityForListMethod = new CMethod
                {
                    IsStatic = true,
                    IsExtensionMethod = true,
                    ReturnType = $"IEnumerable<{alias}.{entityClass.ClassName}>",
                    MethodName = "ToEntity"
                };

                //todo: create method for fully qualified name
                var parameterType = string.Empty;
                
                parameterType =
                    $"IEnumerable<{convertFromModelClass.Namespace.NamespaceName}.{convertFromModelClass.ClassName}>";
                ToEntityForListMethod.Parameter.Add(new CParameter { Type = parameterType, ParameterName = "source" });

                var codeWriter3 = new CodeWriter();
                codeWriter3.WriteLine($"return source.Select(s => s.ToEntity()).ToList();");
                ToEntityForListMethod.CodeSnippet = codeWriter3.ToString();
                extensionsClass.Method.Add(ToEntityForListMethod);
            }

            extensionsClass.NamespaceRef.Add(new CNamespaceRef("System.Collections.Generic"));
            extensionsClass.NamespaceRef.Add(new CNamespaceRef("System.Linq"));
            
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