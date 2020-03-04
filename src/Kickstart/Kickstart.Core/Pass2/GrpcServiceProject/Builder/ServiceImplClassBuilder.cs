using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface IServiceImplClassBuilder
    {
        CClass BuildServiceImplClass(KGrpcProject grpcKProject,  CProtoService protoService, string protoTypesNamespace,
            bool useToEntity = true, bool useToProto = true);
    }

    public class ServiceImplClassBuilder : IServiceImplClassBuilder
    {
        public CClass BuildServiceImplClass(KGrpcProject grpcKProject, CProtoService protoService, string protoTypesNamespace,
           bool useToEntity = true, bool useToProto = true)
        {
            var @class = new CClass($"{protoService.ServiceName}Impl")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{grpcKProject.CompanyName}.{grpcKProject.ProjectName}{grpcKProject.NamespaceSuffix}.{grpcKProject.ProjectSuffix}"
                },
                InheritsFrom = new CClass($"{protoService.ServiceName}.{protoService.ServiceName}Base")
            };
            //todo: add only if needed
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Linq" }
            });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "System.Threading.Tasks" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = "Grpc.Core" } });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = "Company.GrpcCommon.Infrastructure" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = protoTypesNamespace }
            });
            @class.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace { NamespaceName = $"{@class.Namespace}.Query" }
            });

            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{@class.Namespace.NamespaceName}.Config" } });
            @class.NamespaceRef.Add(new CNamespaceRef { ReferenceTo = new CNamespace { NamespaceName = $"{@class.Namespace.NamespaceName}.Auth" } });

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "IMediatorExecutor",
                FieldName = "_executor"
            });

            @class.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                IsReadonly = true,
                FieldType = "AuthenticationSettings",
                FieldName = "_authSettings"
            });

            @class.Constructor.Add(new CConstructor
            {
                AccessModifier = CAccessModifier.Public,
                ConstructorName = @class.ClassName,
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "IMediatorExecutor", ParameterName = "executor"},
                    new CParameter { Type = "AuthenticationSettings", ParameterName = "authSettings"}
                },
                CodeSnippet =
                @"_executor = executor;
                  _authSettings = authSettings;"
            });

            foreach (var rpc in protoService.Rpc)
            {
                var rpcMethod = new CMethod
                {
                    AccessModifier = CAccessModifier.Public,
                    IsOverride = true,
                    IsAsync = true,
                    ReturnType = $"Task<{rpc.Response.MessageName}>",
                    MethodName = rpc.RpcName
                };
                rpcMethod.Parameter.Add(new CParameter
                {
                    Type = $"{rpc.Request.MessageName}",
                    ParameterName = "request"
                });
                rpcMethod.Parameter.Add(new CParameter { Type = "ServerCallContext", ParameterName = "context" });

                var codeWriter = new CodeWriter();
                codeWriter.WriteLine("context.CheckAuthenticated(_authSettings);");
                codeWriter.WriteLine();
                codeWriter.Indent();
                codeWriter.Indent();
                codeWriter.Indent();
                codeWriter.WriteLine($"var result = await _executor.ExecuteAsync(new {rpc.RpcName}Query");
                codeWriter.WriteLine("{");
                codeWriter.Indent();
                var first = true;
                foreach (var pf in rpc.Request.ProtoField)
                {
                    if (!first)
                        codeWriter.WriteLine(",");
                    first = false;
                    codeWriter.Write($"{pf.FieldName} = request.{pf.FieldName}");
                    if (pf.FieldType == GrpcType.__google_protobuf_Timestamp)
                        codeWriter.Write(".ToDateTime()");

                    if (useToEntity && !pf.IsScalar)
                    {
                        if (pf.Repeated)
                        {
                            codeWriter.Write(".Select(p => p.ToModel())");
                        }
                        else
                        {
                            codeWriter.Write(".ToModel()");
                        }
                        
                    }
                }

                codeWriter.WriteLine(string.Empty);
                codeWriter.Unindent();
                codeWriter.WriteLine("}).ConfigureAwait(false);");
                codeWriter.WriteLine("");
                if (rpc.Response.HasFields)
                {
                    //codeWriter.WriteLine("var query = result.FirstOrDefault();");
                    codeWriter.WriteLine();
                    codeWriter.WriteLine($"var response = new {rpc.Response.MessageName}();");

                    codeWriter.WriteLine();

                    codeWriter.WriteLine("if (result != null)");
                    codeWriter.WriteLine("{");
                    codeWriter.Indent();
                    if (rpc.ResponseIsList())
                    {
                        codeWriter.WriteLine("foreach (var r in result)");
                        codeWriter.WriteLine("{");
                        codeWriter.Indent();
                        codeWriter.Write($"response.{rpc.Response.ProtoField.First().FieldName}.Add(r");
                        if (useToProto)
                            codeWriter.Write(".ToProto()");
                        codeWriter.WriteLine("); ");
                        codeWriter.Unindent();
                        codeWriter.WriteLine("}");
                    }
                    else
                    {
                        foreach (var field in rpc.Response.ProtoField)
                        {
                            if (field.Repeated)
                            {
                                if (field.IsScalar)
                                {
                                    codeWriter.WriteLine($"response.{field.FieldName}.Add(result.{field.FieldName});");
                                }
                                else
                                {
                                    codeWriter.WriteLine(
                                        $"response.{field.FieldName}.Add(result.{field.FieldName}.ToProto());");
                                }
                            }
                            else
                            {
                                if (field.IsScalar)
                                {
                                    codeWriter.WriteLine($"response.{field.FieldName} = result.{field.FieldName};");
                                }
                                else
                                {
                                    codeWriter.WriteLine(
                                        $"response.{field.FieldName} = result.{field.FieldName}.ToProto();");
                                }
                            }
                        }
                    }
                    codeWriter.Unindent();
                    codeWriter.WriteLine("}");
                    codeWriter.WriteLine("return response;");
                }
                else
                {
                    codeWriter.WriteLine("");
                    codeWriter.WriteLine($"var response = new {rpc.Response.MessageName}");
                    codeWriter.WriteLine("{ ");
                    codeWriter.WriteLine("};");
                    codeWriter.WriteLine("");
                    codeWriter.WriteLine("return response;");
                }
                rpcMethod.CodeSnippet = codeWriter.ToString();
                @class.Method.Add(rpcMethod);
            }

          

            return @class;
        }
    }
}
