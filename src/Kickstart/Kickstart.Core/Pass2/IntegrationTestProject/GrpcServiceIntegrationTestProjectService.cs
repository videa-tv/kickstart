using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.SampleData;
using Kickstart.Utility;
using Microsoft.Extensions.Configuration;

namespace Kickstart.Pass2.IntegrationTestProject
{
    public class GrpcServiceIntegrationTestProjectService : IGrpcServiceIntegrationTestProjectService
    {
        private KGrpcServiceIntegrationTestProject _grpcMServiceIntegrationTestProject;
        private KGrpcProject _mGrpcProject;
        private readonly IConfigurationRoot _configuration;

        public GrpcServiceIntegrationTestProjectService(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public CProject BuildProject(KGrpcProject mGrpcProject,
            KGrpcServiceIntegrationTestProject grpcMServiceIntegrationTestProject,
            CProject grpcIntegrationTestBusinessProject, bool addProtoRef = false)
        {
            _mGrpcProject = mGrpcProject;
            _grpcMServiceIntegrationTestProject = grpcMServiceIntegrationTestProject;

            var project = new CProject
            {
                ProjectName = grpcMServiceIntegrationTestProject.ProjectFullName,
                ProjectShortName = grpcMServiceIntegrationTestProject.ProjectShortName,
                ProjectFolder = _grpcMServiceIntegrationTestProject.ProjectFolder,
                ProjectType = CProjectType.CsProj,
                ProjectIs = _grpcMServiceIntegrationTestProject.ProjectIs
            };

            project.TemplateProjectPath = @"templates\NetCore31ConsoleApp.csproj";

            /*
            AddProgramClass(project);
            foreach (var protoFile in mGrpcProject.ProtoFile)
            {
                foreach (var protoService in protoFile.GeneratedProtoFile.ProtoService)
                {
                    var @testClass = BuildTestClass(protoService);
                    project.ProjectContent.Add(new SProjectContent()
                    {
                        Content = @testClass,
                        BuildAction = SBuildAction.DoNotInclude,
                        File = new SFile() { Folder = $@"", FileName = $"{@testClass.ClassName}.cs" }
                    });

                }
                if (addProtoRef)
                {
                    AddProtoRpcRef(project, protoFile.GeneratedProtoFile);
                }
            }*/
            AddNugetRefs(project);
            AddProtoBatchFile(project);
            project.ProjectReference.Add(grpcIntegrationTestBusinessProject);
            return project;
        }

        protected void AddNugetRefs(CProject project)
        {
            project.NuGetReference.Add(new CNuGet {NuGetName = "Microsoft.NET.Test.Sdk", Version = "15.3.0"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "Moq", Version = "4.7.99"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "MSTest.TestAdapter", Version = "1.1.18"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "MSTest.TestFramework", Version = "1.1.18"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "MediatR", Version = "4.0.1"});
            project.NuGetReference.Add(new CNuGet { NuGetName = "Google.Protobuf", Version = _configuration.GetValue<string>("Google_Protobuf_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc", Version = _configuration.GetValue<string>("Grpc_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc.Tools", Version = _configuration.GetValue<string>("Grpc_Tools_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc.Core", Version = _configuration.GetValue<string>("Grpc_Core_NugetVersion") });
            project.NuGetReference.Add(new CNuGet { NuGetName = "Grpc.Reflection", Version = _configuration.GetValue<string>("Grpc_Reflection_NugetVersion") });
            
            project.NuGetReference.Add(new CNuGet {NuGetName = "Company.GrpcCommon", Version = _configuration.GetValue<string>("Company_GrpcCommon_NugetVersion") });
        }

        protected void AddProtoBatchFile(CProject project)
        {
            var generateProtoBatchFile = ReadResourceFile("regen-grpc.cmd");
            /*
            project.ProjectContent.Add(new SProjectContent()
            {
                Content = new SBatchFile() { BatchFileContent = generateProtoBatchFile },
                BuildAction = SBuildAction.None,
                File = new SFile() { Folder = $@"", FileName = $"regen-grpc.cmd" }
            });
            */
            project.ProjectContent.Add(new CProjectContent
            {
                Content = new CBatchFile {BatchFileContent = generateProtoBatchFile},
                BuildAction = CBuildAction.None,
                File = new CFile {Folder = $@"Proto\ProtoRef", FileName = $"regen-grpc.cmd"}
            });
        }

        private string ReadResourceFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Kickstart.Core.NetStandard.Boilerplate.{fileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        /*
        private void AddProtoFile(SProject project)
        {
            project.ProjectContent.Add(new SProjectContent()
            {
                Content = _protoFile,
                BuildAction = SBuildAction.None,
                File = new SFile() { Folder = $@"Proto\ProtoRef\Proto", FileName = $"{_grpcMServiceIntegrationTestProject.ProjectName}.proto" }
            });
        }*/

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
                    File = new CFile
                    {
                        Folder = $@"Proto\ProtoRef\Json",
                        FileName = $"{protoRpcRef.ProtoRpc.RpcName}.json"
                    }
                });
            }
        }

        private void AddProgramClass(CProject project)
        {
            var programClass = new CClass("Program")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcMServiceIntegrationTestProject.CompanyName}.{_grpcMServiceIntegrationTestProject.ProjectName}{_grpcMServiceIntegrationTestProject.NamespaceSuffix}.{_grpcMServiceIntegrationTestProject.ProjectSuffix}"
                }
            };

            programClass.NamespaceRef.Add(
                new CNamespaceRef {ReferenceTo = new CNamespace {NamespaceName = "Grpc.Core"}});
            programClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "Google.Protobuf.WellKnownTypes"}
            });
            programClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "Microsoft.VisualStudio.TestTools.UnitTesting"}
            });

            foreach (var protoFile in _mGrpcProject.ProtoFile)
            foreach (var protoService in protoFile.GeneratedProtoFile.ProtoService)
                programClass.NamespaceRef.Add(new CNamespaceRef
                {
                    ReferenceTo = new CNamespace {NamespaceName = $"{protoService.ProtoFile.CSharpNamespace}"}
                });

            var methodSnippet = new CodeWriter();
            var mainMethod =
                new CMethod
                {
                    ReturnType = "void",
                    IsStatic = true,
                    MethodName = "Main",
                    Parameter = new List<CParameter> {new CParameter {Type = "string []", ParameterName = "args"}}
                };

            methodSnippet.WriteLine($@"Console.WriteLine(""Press any key once the service has started"");");
            methodSnippet.WriteLine(@"Console.ReadKey();");
            methodSnippet.WriteLine(@"Channel channel = new Channel(""0.0.0.0:50025"", ChannelCredentials.Insecure); ");


            programClass.Method.Add(mainMethod);

            foreach (var protoFile in _mGrpcProject.ProtoFile)
            foreach (var protoService in protoFile.GeneratedProtoFile.ProtoService)
                methodSnippet.WriteLine(
                    $"var client = new {protoService.ServiceName}.{protoService.ServiceName}Client(channel);");
            mainMethod.CodeSnippet = methodSnippet.ToString();
            methodSnippet.WriteLine(@"Console.WriteLine(""Completed"");");

            methodSnippet.WriteLine(@"Console.ReadKey();");
            project.ProjectContent.Add(new CProjectContent
            {
                Content = programClass,
                BuildAction = CBuildAction.DoNotInclude,
                File = new CFile {Folder = $@"", FileName = $"{programClass.ClassName}.cs"}
            });
        }

        private CClass BuildTestClass(CProtoService protoService)
        {
            var testClass = new CClass($"{protoService.ServiceName}Tests")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcMServiceIntegrationTestProject.CompanyName}.{_grpcMServiceIntegrationTestProject.ProjectName}{_grpcMServiceIntegrationTestProject.NamespaceSuffix}.{_grpcMServiceIntegrationTestProject.ProjectSuffix}"
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
                ReferenceTo = new CNamespace {NamespaceName = "Microsoft.VisualStudio.TestTools.IntegrationTesting"}
            });

            testClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = $"{protoService.ProtoFile.CSharpNamespace}"}
            });

            // private Mock<IMediatorExecutor> _mediatrMock;
            //private Server _server;
            //private Channel _channel;
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

            testClass.Method.Add(BuildSetupMethod());
            testClass.Method.Add(BuildDisposeMethod());

            //todo: finish implementing
            /*
            foreach (var rpc in protoService.Rpc)
            {
                var method = GetTestMethod(rpc);
                @testClass.Method.Add(method);

            }*/
            return testClass;
        }

        private CMethod BuildSetupMethod()
        {
            var setupMethod = new CMethod
            {
                AccessModifier = CAccessModifier.Public,
                ReturnType = "void",
                MethodName = "Setup"
            };
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
                MethodName = methodName,
                Parameter = new List<CParameter>
                {
                    new CParameter
                    {
                        Type = $"{rpc.ProtoService.ServiceName}.{rpc.ProtoService.ServiceName}Client",
                        ParameterName = "client"
                    }
                }
            };
            method.Attribute.Add(new CMethodAttribute {AttributeName = "TestMethod"});

            codeWriter.WriteLine("//Arrange");
            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine($"var request = new {rpc.Request.MessageName}();");

            if (!rpc.OperationIs.HasFlag(COperationIs.Bulk)) //ignore, for now
                foreach (var field in rpc.Request.ProtoField)
                    if (!field.IsScalar)
                    {
                        var childMessage =
                            rpc.ProtoService.ProtoFile.ProtoMessage.First(pm => pm.MessageName == field.FieldName);
                        codeWriter.WriteLine($"request.{field.FieldName} = new {field.FieldName}();");
                        foreach (var childField in childMessage.ProtoField)
                            if (childField.IsScalar)
                                codeWriter.WriteLine(
                                    $"request.{field.FieldName}.{childField.FieldName} = {SampleDataService.GetSampleData(childField, COperationIs.Undefined)}; ");
                    }
                    else
                    {
                        codeWriter.WriteLine(
                            $"request.{field.FieldName} = {SampleDataService.GetSampleData(field, COperationIs.Undefined)}; ");
                    }

            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine("//Act");

            codeWriter.WriteLine(string.Empty);

            codeWriter.WriteLine($"var response = await client.{rpc.RpcName}Async(request);");

            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine("//Assert");

            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine($@"Assert.Fail(""{method.MethodName}() test is not implemented"");");

            method.CodeSnippet = codeWriter.ToString();

            return method;
        }
    }
}