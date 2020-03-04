using System.Collections.Generic;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.IntegrationTestProject
{
    internal class GrpcServiceIntegrationTestDbProjectService
    {
        private KGrpcServiceIntegrationTestDbProject _grpcServiceIntegrationTestDbProject;


        public CProject BuildProject(KGrpcServiceIntegrationTestDbProject grpcServiceIntegrationTestDbProject)
        {
            _grpcServiceIntegrationTestDbProject = grpcServiceIntegrationTestDbProject;
            var project = new CProject
            {
                ProjectName = grpcServiceIntegrationTestDbProject.ProjectFullName,
                ProjectShortName = grpcServiceIntegrationTestDbProject.ProjectShortName,
                ProjectFolder = grpcServiceIntegrationTestDbProject.ProjectFolder,
                ProjectType = CProjectType.CsProj,
                ProjectIs = CProjectIs.Test | CProjectIs.Client
            };
            /*
            project.TemplateProjectPath = @"NetCore22ConsoleApp.csproj";
             project.NuGetReference.Add(new SNuGet() { NuGetName = "Microsoft.NET.Test.Sdk", Version = "15.3.0" });
            project.NuGetReference.Add(new SNuGet() { NuGetName = "Moq", Version = "4.7.99" });
            project.NuGetReference.Add(new SNuGet() { NuGetName = "MSTest.TestAdapter", Version = "1.1.18" });
            project.NuGetReference.Add(new SNuGet() { NuGetName = "MSTest.TestFramework", Version = "1.1.18" });

            AddProgramClass(project);
            foreach (var protoService in _protoFile.ProtoService)
            {
                var @testClass = BuildTestClass(protoService);
                project.ProjectContent.Add(new SProjectContent()
                {
                    Content = @testClass,
                    BuildAction = SBuildAction.DoNotInclude,
                    File = new SFile() { Folder = $@"", FileName = $"{@testClass.ClassName}.cs" }
                });

            }
            
            project.ProjectReference.Add(grpcIntegrationTestBusinessProject);*/
            return project;
        }

        private CClass BuildTestClass()
        {
            var testClass = new CClass($"DbTests")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcServiceIntegrationTestDbProject.CompanyName}.{_grpcServiceIntegrationTestDbProject.ProjectName}{_grpcServiceIntegrationTestDbProject.NamespaceSuffix}.{_grpcServiceIntegrationTestDbProject.ProjectSuffix}"
                } 
            };
            testClass.ClassAttribute.Add(new CClassAttribute {AttributeName = "TestClass"});

            testClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "System.Threading.Tasks"}
            });
            testClass.NamespaceRef.Add(new CNamespaceRef {ReferenceTo = new CNamespace {NamespaceName = "Moq"}});
            testClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "Microsoft.VisualStudio.TestTools.IntegrationTesting"}
            });

            //@testClass.NamespaceRef.Add(new SNamespaceRef { ReferenceTo = new SNamespace { NamespaceName = $"{protoService.ProtoFile.CSharpNamespace}" } });


            testClass.Method.Add(BuildSetupMethod());
            testClass.Method.Add(BuildDisposeMethod());
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


            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine("//Act");

            codeWriter.WriteLine(string.Empty);


            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine("//Assert");

            codeWriter.WriteLine(string.Empty);


            method.CodeSnippet = codeWriter.ToString();

            return method;
        }
    }
}