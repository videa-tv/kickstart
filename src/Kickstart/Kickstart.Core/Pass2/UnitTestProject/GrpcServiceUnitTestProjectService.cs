using System;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.UnitTestProject
{
    public interface IGrpcServiceUnitTestProjectService
    {
        CProject BuildProject(KGrpcProject mGrpcProject, KGrpcServiceUnitTestProject grpcMServiceUnitTestProject,
            CProject grpcProject, bool addProtoRef = false);
    }

    public class GrpcServiceUnitTestProjectService : IGrpcServiceUnitTestProjectService
    {
        private KGrpcServiceUnitTestProject _grpcMServiceUnitTestProject;

        private KGrpcProject _mGrpcProject;

        public CProject BuildProject(KGrpcProject mGrpcProject, KGrpcServiceUnitTestProject grpcMServiceUnitTestProject,
            CProject grpcProject, bool addProtoRef = false)
        {
            _mGrpcProject = mGrpcProject;
            _grpcMServiceUnitTestProject = grpcMServiceUnitTestProject;
            var project = new CProject
            {
                ProjectName = grpcMServiceUnitTestProject.ProjectFullName,
                ProjectShortName = grpcMServiceUnitTestProject.ProjectShortName,
                ProjectFolder = grpcMServiceUnitTestProject.ProjectFolder,
                ProjectType = CProjectType.CsProj,
                ProjectIs = grpcMServiceUnitTestProject.ProjectIs
            };

            project.TemplateProjectPath = @"templates\NetCore31ConsoleApp.csproj";
            project.NuGetReference.Add(new CNuGet {NuGetName = "Microsoft.NET.Test.Sdk", Version = "15.3.0"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "Moq", Version = "4.7.99"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "MSTest.TestAdapter", Version = "1.1.18"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "MSTest.TestFramework", Version = "1.1.18"});

            //AddProgramClass(project);
            foreach (var protoFile in _mGrpcProject.ProtoFile)
            {
                foreach (var protoService in protoFile.GeneratedProtoFile.ProtoService)
                {
                    var testClass = BuildTestClass(protoService);
                    project.ProjectContent.Add(new CProjectContent
                    {
                        Content = testClass,
                        BuildAction = CBuildAction.DoNotInclude,
                        File = new CFile {Folder = $@"", FileName = $"{testClass.ClassName}.cs"}
                    });
                }
                if (addProtoRef)
                    AddProtoRpcRef(project, protoFile.GeneratedProtoFile);
            }
            project.ProjectReference.Add(grpcProject);
            return project;
        }

        private void AddProtoRpcRef(CProject project, CProtoFile protoFile)
        {
            foreach (var protoService in protoFile.ProtoService)
            foreach (var rpc in protoService.Rpc)
            {
                var protoRpcRef = new CProtoRpcRef {Direction = CProtoRpcRefDataDirection.Undefined, ProtoRpc = rpc};
                project.ProjectContent.Add(new CProjectContent
                {
                    Content = protoRpcRef,
                    BuildAction = CBuildAction.DoNotInclude,
                    File = new CFile {Folder = $@"Proto", FileName = $"{protoRpcRef.ProtoRpc.RpcName}.json"}
                });
            }
        }


        private CClass BuildTestClass(CProtoService protoService)
        {
            var testClass = new CClass($"{protoService.ServiceName}Tests")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcMServiceUnitTestProject.CompanyName}.{_grpcMServiceUnitTestProject.ProjectName}{_grpcMServiceUnitTestProject.NamespaceSuffix}.{_grpcMServiceUnitTestProject.ProjectSuffix}"
                }
            };
            testClass.ClassAttribute.Add(new CClassAttribute {AttributeName = "TestClass"});

            testClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "System.Threading.Tasks"}
            });
            testClass.NamespaceRef.Add(new CNamespaceRef {ReferenceTo = new CNamespace {NamespaceName = "Moq"}});
            testClass.NamespaceRef.Add(new CNamespaceRef {ReferenceTo = new CNamespace {NamespaceName = "Grpc.Core"}});
            testClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "Google.Protobuf.WellKnownTypes"}
            });
            testClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "Microsoft.VisualStudio.TestTools.UnitTesting"}
            });
            testClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "Company.GrpcCommon.Infrastructure"}
            });

            testClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = $"{protoService.ProtoFile.CSharpNamespace}"}
            });


            testClass.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                FieldType = "Mock<IMediatorExecutor>",
                FieldName = "_mediatrMock"
            });
            testClass.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                FieldType = "Server",
                FieldName = "_server"
            });
            testClass.Field.Add(new CField
            {
                AccessModifier = CAccessModifier.Private,
                FieldType = "Channel",
                FieldName = "_channel"
            });

            testClass.Method.Add(BuildInitializeMethod());
            testClass.Method.Add(BuildDisposeMethod());
            foreach (var rpc in protoService.Rpc)
            {
                var method = GetTestMethod(rpc);
                testClass.Method.Add(method);
            }
            return testClass;
        }

        private CMethod BuildInitializeMethod()
        {
            var setupMethod = new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "Initialize"
            };
            setupMethod.CodeSnippet = "//todo: create mock data service";

            setupMethod.Attribute.Add(new CMethodAttribute {AttributeName = "TestInitialize"});
            return setupMethod;
        }

        private CMethod BuildDisposeMethod()
        {
            var disposeMethod = new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "Dispose"
            };

            disposeMethod.Attribute.Add(new CMethodAttribute {AttributeName = "TestCleanup"});
            disposeMethod.CodeSnippet =
                $@" _channel.ShutdownAsync().Wait();
            _server.ShutdownAsync().Wait();";

            return disposeMethod;
        }

        private CMethod GetTestMethod(CProtoRpc rpc)
        {
            var codeWriter = new CodeWriter();

            var methodName = $"{rpc.RpcName}_Success";

            var method = new CMethod
            {
                ReturnType = "Task",
                IsAsync = true,
                IsStatic = false,
                MethodName = methodName
                //  Parameter = new List<SParameter> { new SParameter { Type = $"{rpc.ProtoService.ServiceName}.{rpc.ProtoService.ServiceName}Client", ParameterName = "client" } }
            };
            method.Attribute.Add(new CMethodAttribute {AttributeName = "TestMethod"});

            codeWriter.WriteLine("//Arrange");
            codeWriter.WriteLine(string.Empty);

            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine("//Act");

            codeWriter.WriteLine(string.Empty);


            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine("//Assert");

            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine($@"Assert.Fail(""{method.MethodName}() test is not implemented"");");

            method.CodeSnippet = codeWriter.ToString();

            return method;
        }

        private string GetSampleData(CProtoMessageField childField)
        {
            switch (childField.FieldType)
            {
                case GrpcType.__string:
                    return $@"""{Guid.NewGuid().ToString()}""";
                    ;
                case GrpcType.__int32:
                    return new Random().Next().ToString();
                case GrpcType.__int64:
                    return new Random().Next().ToString();

                case GrpcType.__google_protobuf_Timestamp:
                    return $@"Timestamp.FromDateTime(DateTime.SpecifyKind(  DateTime.Parse(""{
                            DateTime.UtcNow.ToString("o")
                        }""), DateTimeKind.Utc))";
                case GrpcType.__bool:
                    return $@"true";
            }
            return "null";
        }
    }
}