using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass2.SampleData;
using Kickstart.Utility;

namespace Kickstart.Pass2.IntegrationTestProject
{
    public class GrpcServiceClientTestProjectService : IGrpcServiceClientTestProjectService
    {
        private KGrpcServiceClientTestProject _grpcMServiceClientTestProject;

        protected KSolution _kSolution;

        private KGrpcProject _mGrpcProject;
        private readonly IGrpcPortService _grpcPortService;
        public GrpcServiceClientTestProjectService(IGrpcPortService grpcPortService)
        {
            _grpcPortService = grpcPortService;
        }
        public CProject BuildProject(KSolution kSolution, KGrpcProject mGrpcProject,
            KGrpcServiceClientTestProject grpcMServiceClientTestProject, CProject grpcProject, bool addProtoRef = false)
        {
            _kSolution = kSolution;
            _mGrpcProject = mGrpcProject;
            _grpcMServiceClientTestProject = grpcMServiceClientTestProject;
            var project = new CProject
            {
                ProjectName = grpcMServiceClientTestProject.ProjectFullName,
                ProjectShortName = grpcMServiceClientTestProject.ProjectShortName,
                ProjectFolder = grpcMServiceClientTestProject.ProjectFolder,
                ProjectType = CProjectType.CsProj,
                ProjectIs = CProjectIs.Test | CProjectIs.Client,
                StartupObject =
                    $"{grpcMServiceClientTestProject.CompanyName}.{grpcMServiceClientTestProject.ProjectName}{grpcMServiceClientTestProject.NamespaceSuffix}.{grpcMServiceClientTestProject.ProjectSuffix}.Program"
            };

            project.TemplateProjectPath = @"templates\NetCore31ConsoleApp.csproj";
            project.NuGetReference.Add(new CNuGet {NuGetName = "Microsoft.NET.Test.Sdk", Version = "15.3.0"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "Moq", Version = "4.7.99"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "MSTest.TestAdapter", Version = "1.1.18"});
            project.NuGetReference.Add(new CNuGet {NuGetName = "MSTest.TestFramework", Version = "1.1.18"});


            var testClasses = new List<CClass>();
            foreach (var protoFile in _mGrpcProject.ProtoFile)
            {
                foreach (var protoService in protoFile.GeneratedProtoFile.ProtoService)
                {
                    var testClass = BuildTestClass(protoService);
                    testClasses.Add(testClass);
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
            AddProgramClass(project, grpcProject, testClasses);


            AddProtoBatchFile(project);
            project.ProjectReference.Add(grpcProject);
            return project;
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
            var resourceName = $"Kickstart.Core.NetStandard.Boilerplate.Grpc.{fileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void AddProtoFile(CProject project, CProtoFile protoFile)
        {
            project.ProjectContent.Add(new CProjectContent
            {
                Content = protoFile,
                BuildAction = CBuildAction.None,
                File = new CFile
                {
                    Folder = $@"Proto\ProtoRef\Proto",
                    FileName = $"{_grpcMServiceClientTestProject.ProjectName}.proto"
                }
            });
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
                    File = new CFile
                    {
                        Folder = $@"Proto\ProtoRef\Json",
                        FileName = $"{protoRpcRef.ProtoRpc.RpcName}.json"
                    }
                });
            }
        }

        private void AddProgramClass(CProject project, CProject grpcProject, List<CClass> testClasses)
        {
            var programClass = new CClass("Program")
            {
                Namespace = new CNamespace
                {
                    NamespaceName =
                        $"{_grpcMServiceClientTestProject.CompanyName}.{_grpcMServiceClientTestProject.ProjectName}{_grpcMServiceClientTestProject.NamespaceSuffix}.{_grpcMServiceClientTestProject.ProjectSuffix}"
                }
            };

            programClass.NamespaceRef.Add(new CNamespaceRef
            {
                ReferenceTo = new CNamespace {NamespaceName = "System.Diagnostics"}
            });
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

            methodSnippet.WriteLine("bool includeDeletes = false;");

            /* methodSnippet.WriteLine(
                $@"Console.WriteLine(""Press any key once the service ({
                        grpcProject.ProjectName
                    }) has started.\r\nPress 'D' to include deletes"");");
                    */
            methodSnippet.WriteLine(
                $@"Console.WriteLine(""Press any key once the service ({
                        grpcProject.ProjectName
                    }) has started."");");

            methodSnippet.WriteLine(@"var key = Console.ReadKey().KeyChar;");
            /*methodSnippet.WriteLine(@"includeDeletes = key.ToString().ToLower() == ""d"";");
            methodSnippet.WriteLine(@"Console.WriteLine(""Input current DB identity value (default = 1000):"");");
            methodSnippet.WriteLine(@"int currentDbIdentityValue = 1000;");
            methodSnippet.WriteLine(@"if (!Int32.TryParse(Console.ReadLine(), out currentDbIdentityValue))");
            methodSnippet.WriteLine("{");
            methodSnippet.Indent();
            methodSnippet.WriteLine("currentDbIdentityValue = 1000;");
            methodSnippet.Unindent();
            methodSnippet.WriteLine("}");
            */
            var portNumber = _grpcPortService.GeneratePortNumber(_kSolution.SolutionName);
            methodSnippet.WriteLine(
                $@"Channel channel = new Channel(""127.0.0.1:{portNumber++}"", ChannelCredentials.Insecure); ");


            programClass.Method.Add(mainMethod);

            foreach (var protoFile in _mGrpcProject.ProtoFile)
            foreach (var protoService in protoFile.GeneratedProtoFile.ProtoService)
                methodSnippet.WriteLine(
                    $"var client{protoService.ServiceName} = new {protoService.ServiceName}.{protoService.ServiceName}Client(channel);");
            //create an instance of the class, and call each method
            foreach (var testClass in testClasses)
            {
                var protoService = testClass.DerivedFrom as CProtoService;
                //methodSnippet.WriteLine(
                  //  $"var {testClass.ClassNameAsCamelCase} = new {testClass.ClassName}(currentDbIdentityValue);");
                methodSnippet.WriteLine(
               $"var {testClass.ClassNameAsCamelCase} = new {testClass.ClassName}(0);");

                foreach (var method in testClass.Method)
                {
                    methodSnippet.WriteLine();
                    if (method.MethodIs == COperationIs.Delete)
                    {
                        methodSnippet.WriteLine("if (includeDeletes)");
                        methodSnippet.WriteLine("{");
                        methodSnippet.Indent();
                    }

                    methodSnippet.WriteLine("try");
                    methodSnippet.WriteLine("{");
                    methodSnippet.Indent();
                    methodSnippet.WriteLine("var sw = Stopwatch.StartNew();");
                    if (method.IsAsync)
                        methodSnippet.WriteLine(
                            $"{testClass.ClassNameAsCamelCase}.{method.MethodName}(client{protoService.ServiceName}).Wait();");
                    else
                        methodSnippet.WriteLine(
                            $"{testClass.ClassNameAsCamelCase}.{method.MethodName}(client{protoService.ServiceName});");
                    methodSnippet.WriteLine($" Console.BackgroundColor = ConsoleColor.DarkGreen;");
                    methodSnippet.WriteLine(
                        $@"Console.WriteLine($""{method.MethodName}() completed in {{sw.ElapsedMilliseconds}} ms"");");
                    methodSnippet.Unindent();


                    methodSnippet.WriteLine("}");
                    methodSnippet.WriteLine("catch");

                    methodSnippet.WriteLine("{");
                    methodSnippet.Indent();
                    methodSnippet.WriteLine($" Console.BackgroundColor = ConsoleColor.Red;");
                    methodSnippet.WriteLine($@"Console.WriteLine(""{method.MethodName}() failed"");");
                    methodSnippet.Unindent();
                    methodSnippet.WriteLine("}");

                    if (method.MethodIs == COperationIs.Delete)
                    {
                        methodSnippet.Unindent();
                        methodSnippet.WriteLine("}");
                    }
                }
            }
            methodSnippet.WriteLine($" Console.BackgroundColor = ConsoleColor.Black;");
            methodSnippet.WriteLine(@"Console.WriteLine(""Completed. Press any key to exit."");");

            methodSnippet.WriteLine(@"Console.ReadKey();");
            ;
            mainMethod.CodeSnippet = methodSnippet.ToString();
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
                        $"{_grpcMServiceClientTestProject.CompanyName}.{_grpcMServiceClientTestProject.ProjectName}{_grpcMServiceClientTestProject.NamespaceSuffix}.{_grpcMServiceClientTestProject.ProjectSuffix}"
                },
                DerivedFrom = protoService
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
                ReferenceTo = new CNamespace {NamespaceName = $"{protoService.ProtoFile.CSharpNamespace}"}
            });

            // private Mock<IMediatorExecutor> _mediatrMock;
            //private Server _server;
            //private Channel _channel;
            //testClass.Field.Add(new SField { AccessModifier = SAccessModifier.Private, FieldType = "Mock<IMediatorExecutor>", FieldName = "_mediatrMock" });
            // testClass.Field.Add(new SField { AccessModifier = SAccessModifier.Private, FieldType = "Server", FieldName = "_server" });
            // testClass.Field.Add(new SField { AccessModifier = SAccessModifier.Private, FieldType = "Channel", FieldName = "_channel" });

            //testClass.Method.Add(BuildSetupMethod());
            //testClass.Method.Add(BuildDisposeMethod());
            testClass.Field.Add(new CField
            {
                IsReadonly = true,
                AccessModifier = CAccessModifier.Private,
                FieldType = "int",
                FieldName = "_currentDbIdentityValue",
                DefaultValue = "1000"
            });

            testClass.Constructor.Add(new CConstructor
            {
                ConstructorName = testClass.ClassName,
                AccessModifier = CAccessModifier.Public,
                Parameter = new List<CParameter>
                {
                    new CParameter {Type = "int", ParameterName = "currentDbIdentityValue"}
                },
                CodeSnippet = "_currentDbIdentityValue = currentDbIdentityValue;"
            });

            foreach (var rpc in protoService.Rpc)
                if (rpc.DerivedFrom is CStoredProcedure &&
                    (rpc.DerivedFrom as CStoredProcedure).DataOperationIs.HasFlag(COperationIs.Add) &&
                    (rpc.DerivedFrom as CStoredProcedure).DataOperationIs.HasFlag(COperationIs.Update)
                )
                {
                    var methodAdd = GetTestMethod(rpc, COperationIs.Add);
                    if (methodAdd == null)
                        continue;
                    testClass.Method.Add(methodAdd);

                    var methodUpdate = GetTestMethod(rpc, COperationIs.Update);
                    if (methodUpdate == null)
                        continue;
                    testClass.Method.Add(methodUpdate);
                }
                else if (rpc.DerivedFrom is CStoredProcedure &&
                         (rpc.DerivedFrom as CStoredProcedure).DataOperationIs.HasFlag(COperationIs.Delete))
                {
                    var methodDelete = GetTestMethod(rpc, COperationIs.Delete);
                    if (methodDelete == null)
                        continue;
                    testClass.Method.Add(methodDelete);
                }
                else
                {
                    var method = GetTestMethod(rpc, COperationIs.Undefined);
                    if (method == null)
                        continue;
                    testClass.Method.Add(method);
                }
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

        private CMethod GetTestMethod(CProtoRpc rpc, COperationIs operationIs)
        {
            if (rpc.OperationIs.HasFlag(COperationIs.Bulk))
                return null;
            var codeWriter = new CodeWriter();

            var methodName = $"{rpc.RpcName}";
            if (operationIs == COperationIs.Add)
                methodName += "Add";
            else if (operationIs == COperationIs.Update)
                methodName += "Update";
            var method = new CMethod
            {
                MethodIs = operationIs,
                DerivedFrom = rpc,
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
            //method.Attribute.Add(new SMethodAttribute { AttributeName = "TestMethod" });

            codeWriter.WriteLine("//Arrange");
            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine($"var request = new {rpc.Request.MessageName}();");

            if (!rpc.OperationIs.HasFlag(COperationIs.Bulk)) //ignore, for now
                foreach (var field in rpc.Request.ProtoField)
                    if (field.FieldType == GrpcType.__enum)
                    {
                        var childEnum = rpc.ProtoService.ProtoFile.ProtoEnum.First(pm =>
                            pm.EnumName.ToLower() == field.EnumType.ToLower());
                    }
                    else if (field.FieldType == GrpcType.__map)
                    {
                        //todo:
                    }
                    else if (field.FieldType == GrpcType.__message)
                    {
                        var childMessage = rpc.ProtoService.ProtoFile.ProtoMessage.First(pm =>
                            pm.MessageName.ToLower() == field.MessageType.ToLower());
                        if (!field.Repeated)
                        {
                            codeWriter.WriteLine($"request.{field.FieldName} = new {field.FieldName}();");
                            foreach (var childField in childMessage.ProtoField)
                                if (childField.IsScalar && !childField.Repeated)
                                    codeWriter.WriteLine(
                                        $"request.{field.FieldName}.{childField.FieldName} = {SampleDataService.GetSampleData(childField, operationIs)}; ");
                                else if (childField.IsScalar && childField.Repeated)
                                    codeWriter.WriteLine(
                                        $"request.{field.FieldName}.{childField.FieldName}.Add({SampleDataService.GetSampleData(childField, operationIs)}); ");
                        }
                        else
                        {
                            codeWriter.WriteLine($"var {field.FieldName.ToLower()} = new {field.MessageType}();");
                            foreach (var childField in childMessage.ProtoField)
                                if (childField.IsScalar && !childField.Repeated)
                                    codeWriter.WriteLine(
                                        $"{field.FieldName.ToLower()}.{childField.FieldName} = {SampleDataService.GetSampleData(childField, operationIs)}; ");
                                else if (childField.IsScalar && childField.Repeated)
                                    codeWriter.WriteLine(
                                        $"{field.FieldName.ToLower()}.{childField.FieldName}.Add({SampleDataService.GetSampleData(childField, operationIs)}); ");
                            codeWriter.WriteLine($"request.{field.FieldName}.Add({field.FieldName.ToLower()});");
                        }
                    }
                    else
                    {
                        if (field.Repeated)
                        {
                            codeWriter.WriteLine(
                                $"request.{field.FieldName}.Add({SampleDataService.GetSampleData(field, operationIs)}); ");
                        }
                        else
                        {
                            if (rpc.DerivedFrom is CStoredProcedure &&
                                (rpc.DerivedFrom as CStoredProcedure).DataOperationIs.HasFlag(COperationIs.Delete))
                                if (field.FieldType == GrpcType.__int64 || field.FieldType == GrpcType.__int32)
                                    codeWriter.WriteLine($"request.{field.FieldName}  = _currentDbIdentityValue;");
                                else
                                    codeWriter.WriteLine(
                                        $"request.{field.FieldName}  = {SampleDataService.GetSampleData(field, operationIs)};");
                            else
                                codeWriter.WriteLine(
                                    $"request.{field.FieldName} = {SampleDataService.GetSampleData(field, operationIs)}; ");
                        }
                    }

            codeWriter.WriteLine(string.Empty);
            codeWriter.WriteLine("//Act");

            codeWriter.WriteLine(string.Empty);

            codeWriter.WriteLine($"var response = await client.{rpc.RpcName}Async(request);");

            codeWriter.WriteLine(string.Empty);
            //codeWriter.WriteLine("//Assert");
            if (rpc.ResponseIsList())
            {
                codeWriter.WriteLine($"Console.BackgroundColor = ConsoleColor.DarkGreen;");
                codeWriter.WriteLine(
                    $@"Console.WriteLine($""{{response.{rpc.Response.ProtoField.First().FieldName}.Count}} {rpc.Response.ProtoField.First().FieldName} records returned"");");
            }
            else
            {
                codeWriter.WriteLine($"Console.BackgroundColor = ConsoleColor.DarkGreen;");
                foreach (var field in rpc.Response.ProtoField)
                {
                   
                    codeWriter.WriteLine(
                        $@"Console.WriteLine($""{{response.{field.FieldName}}}"");");
                }
                
            }
            //codeWriter.WriteLine(string.Empty);
            // codeWriter.WriteLine($@"Assert.Fail(""{method.MethodName}() test is not implemented"");");

            method.CodeSnippet = codeWriter.ToString();

            return method;
        }
    }
}