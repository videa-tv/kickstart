using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;
using Microsoft.Extensions.Logging;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface IEntityToModelCClassConverter
    {
        void AddEntityToModelMethods(CClass extensionsClass, CProtoFile protoFile, IList<CClass> modelClasses,
            List<CClass> convertFromEntityClasses,
            string modelNamespace);
    }

    internal class EntityToModelCClassConverter : IEntityToModelCClassConverter
    {
        private readonly ILogger<EntityToModelCClassConverter> _logger;
        public EntityToModelCClassConverter(ILogger<EntityToModelCClassConverter> logger)
        {
            _logger = logger;
        }
        public void AddEntityToModelMethods(CClass extensionsClass, CProtoFile protoFile, IList<CClass> modelClasses,
            List<CClass> convertFromEntityClasses,
            string modelNamespace)
        {
            var alias = "Model";
            
            extensionsClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System" } });
            extensionsClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "System.Linq" } });

            extensionsClass.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { Alias = alias, NamespaceName = modelNamespace } });
            //todo: should we use SProtoFile instead?

            foreach (var convertFromClass in convertFromEntityClasses)
            {
                //this is a little "fuzzy" since one stored proc could produce 2 "Models". One for the params and another for resultset
                var convertFrompProtoRpc = convertFromClass.DerivedFrom?.DerivedFrom as CProtoMessage;
                //var modelClass = modelClasses.FirstOrDefault(mc => (mc.DerivedFrom?.DerivedFrom as CProtoMessage)?.Rpc == convertFrompProtoRpc);
                if (convertFrompProtoRpc == null)
                {
                    _logger.LogWarning($"convertFrompProtoRpc is null");
                    continue;
                    
                }
                var modelClass =
                    modelClasses.FirstOrDefault(mc => mc.ClassName == convertFrompProtoRpc.DomainModelNameForOutput);

                if (modelClass == null)
                    continue;
                var toModelMethod = new CMethod
                {
                    IsStatic = true,
                    IsExtensionMethod = true,
                    ReturnType = $"{alias}.{modelClass.ClassName}",
                    MethodName = "ToModel"
                };

                //todo: create method for fully qualified name
                toModelMethod.Parameter.Add(new CParameter
                {
                    Type = $"{convertFromClass.Namespace.NamespaceName}.{convertFromClass.ClassName}",//$"Model.{convertFrompProtoRpc.DomainModelNameForOutput}",// $"{convertFromClass.Namespace.NamespaceName}.{convertFromClass.ClassName}",
                    ParameterName = "source"
                });

                var codeWriter = new CodeWriter();
                //var protoMessage = protoFile.ProtoMessage.FirstOrDefault(m => m.MessageName == convertFromClass.ClassName);


                if (modelClass == null)
                {
                    codeWriter.WriteLine("throw new NotImplementedException();");
                }
                else
                {

                    codeWriter.WriteLine($"return new {alias}.{ modelClass.ClassName}");
                    codeWriter.WriteLine("{");
                    codeWriter.Indent();
                    
                    var first = true;
                    foreach (var convertToProperty in modelClass.Property)
                    {
                        var convertFromProperty =
                            convertFromClass.Property.FirstOrDefault(p =>
                                p.PropertyName == convertToProperty.PropertyName);
                        if (convertFromProperty == null)
                            continue;

                        if (!first)
                            codeWriter.WriteLine(",");
                        first = false;

                        
                        codeWriter.Write($"{convertToProperty.PropertyName} = source.{convertFromProperty.PropertyName}");
                    }
                    
                    codeWriter.Unindent();
                    codeWriter.WriteLine("};");
                }
                toModelMethod.CodeSnippet = codeWriter.ToString();
                extensionsClass.Method.Add(toModelMethod);

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