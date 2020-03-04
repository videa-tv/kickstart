using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Google.Protobuf.Reflection;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Configuration;

namespace Kickstart.Pass1.Service
{
    public class ProtoToKProtoConverter : IProtoToKProtoConverter
    {
        private const string TempFilePrefix = "temp";
        private readonly IConfiguration _configuration;
        bool MergeIfPossible { get; set; } = false;
        public ProtoToKProtoConverter(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public KProtoFile BuildProtoFileFromKSolution(KSolution solution)
        {
            if (string.IsNullOrWhiteSpace(solution.ProtoFileText))
                return null;
            try
            {
                var kProtoFile = Convert(solution.ProtoFileName, solution.ProtoFileText);
                return kProtoFile;
            }
            catch (ProtoCompileException)
            {
                return null;
            }
        }
        public KProtoFile Convert(string protoFileName, string protoFileContent)
        {
            //1. save file to temp folder
            //2. run protoc compiler, convert to C# class
            //3. Compile the code in memory
            //4. Read the .Descriptor
            //5. Converto SProtoFile
            var codePath = GenerateCodeFromProto(protoFileContent);
            var code = GetGeneratedCode(codePath);
            var assembly = CompileCodeToAssembly(code);
            var descriptors = GetFileDescriptorsFromAssembly(assembly);
            var messagesAdded = new List<CProtoMessage>();
            var protoFile = new CProtoFile();
            protoFile.SourceProtoText = protoFileContent;

            foreach (var descriptorPair in descriptors)
            {
                var descriptor = descriptorPair.Value;
                protoFile.CSharpNamespace = descriptorPair.Key;

                protoFile.Import.Add("google/protobuf/timestamp.proto");
                protoFile.Import.Add("google/protobuf/duration.proto");
                protoFile.Import.Add("google/protobuf/descriptor.proto");

                protoFile.Option.Add($@"csharp_namespace = ""{protoFile.CSharpNamespace}"";");
                protoFile.Option.Add($@"(version) = ""1.0.0"";");

                foreach (var service in descriptor.Services)
                {
                    var protoService = new CProtoService(protoFile) {ServiceName = service.Name};
                    foreach (var method in service.Methods)
                    {
                        var protoServiceMethod = new CProtoRpc(protoService)
                        {
                            RpcName = method.Name
                        };
                        protoService.Rpc.Add(protoServiceMethod);
                        protoServiceMethod.Request = new CProtoMessage (protoServiceMethod) {MessageName = method.InputType.Name};
                        messagesAdded.Add(protoServiceMethod.Request);

                        foreach (var field in method.InputType.Fields.InFieldNumberOrder())
                        {
                            //protoServiceMethod.Request.ProtoField.Add(BuildProtoMessageField(field));
                            ProcessField(field, descriptor, protoServiceMethod.Request, messagesAdded);

                        }

                        protoServiceMethod.Response = new CProtoMessage (protoServiceMethod) {MessageName = method.OutputType.Name};
                        messagesAdded.Add(protoServiceMethod.Response);
                        foreach (var field in method.OutputType.Fields.InFieldNumberOrder())
                        {
                            //protoServiceMethod.Response.ProtoField.Add(BuildProtoMessageField(field));
                            ProcessField(field, descriptor, protoServiceMethod.Response, messagesAdded);

                        }
                    }
                    protoFile.ProtoService.Add(protoService);
                }
                foreach (var enumType in descriptor.EnumTypes)
                {
                    if (protoFile.ProtoEnum.Exists(pe => pe.EnumName == enumType.Name))
                        continue;
                    var protoEnum = new CProtoEnum {EnumName = enumType.Name};
                    foreach (var enumTypeItem in enumType.Values)
                    {
                        var enumValue = new CProtoEnumValue
                        {
                            EnumValueName = enumTypeItem.Name,
                            EnumValueNumber = enumTypeItem.Number
                        };
                        protoEnum.EnumValue.Add(enumValue);
                    }
                    protoFile.ProtoEnum.Add(protoEnum);
                }
                foreach (var message in descriptor.MessageTypes)
                {
                    if (messagesAdded.Exists(pm => pm.MessageName == message.Name))
                        continue;
                    var protoMessage = new CProtoMessage(null) {MessageName = message.Name};

                    foreach (var field in message.Fields.InFieldNumberOrder())
                    {
                        ProcessField(field, descriptor, protoMessage, messagesAdded);
                    }

                    protoFile.ProtoMessage.Add(protoMessage);
                    messagesAdded.Add(protoMessage);
                }
                foreach (var dependency in descriptor.Dependencies)
                foreach (var message in dependency.MessageTypes)
                {
                    if (messagesAdded.Exists(pm => pm.MessageName == message.Name))
                        continue;
                    var protoMessage = new CProtoMessage(null) {MessageName = message.Name, IsExternal = true};
                    protoFile.ProtoMessage.Add(protoMessage);
                    messagesAdded.Add(protoMessage);
                }
            }
            return new KProtoFile
            {
                ProtoFileFile = protoFileName,
                GeneratedProtoFile = protoFile
            };
            
        }

        private void ProcessField(FieldDescriptor field, FileDescriptor descriptor, CProtoMessage protoMessage,
            List<CProtoMessage> messagesAdded)
        {
            bool processed = false;
            var messageType = GoogleGrpcTypeToGrpcType(field);
            if (MergeIfPossible)
            {
                if (messageType == GrpcType.__message && !field.IsRepeated)
                {
                    //conservative merge:
                    //merge the message into parent if it doesn't have any of its own child messages and its not a collection
                    var childMessage = descriptor.MessageTypes.First(m => m.Name == field.MessageType.Name);
                    var childFields = childMessage.Fields.InFieldNumberOrder().ToList();

                    if (childFields.All(f => GoogleGrpcTypeToGrpcType(f) != GrpcType.__message))
                    {
                        foreach (var childField in childMessage.Fields.InFieldNumberOrder())
                        {
                            protoMessage.ProtoField.Add(BuildProtoMessageField(childField));
                        }

                        processed = true;

                        messagesAdded.Add(new CProtoMessage(null) { MessageName = childMessage.Name });
                    }
                }
            }

            if (!processed)
            {
                protoMessage.ProtoField.Add(BuildProtoMessageField(field));
            }
        }

        private CProtoMessageField BuildProtoMessageField(FieldDescriptor field)
        {
            return new CProtoMessageField(null)
            {
                FieldName = field.Name,
                Repeated = field.IsRepeated && !field.IsMap,
                IsScalar = GoogleGrpcTypeToGrpcType(field) != GrpcType.__message &&
                           GoogleGrpcTypeToGrpcType(field) != GrpcType.__enum && !field.IsMap,
                FieldType = GoogleGrpcTypeToGrpcType(field),
                MessageType = GoogleGrpcMessageTypeToMessageType(field),
                EnumType = field.FieldType == FieldType.Enum ? field.EnumType.Name : null,
                MapType = field.IsMap == true ? "map<string,string>" : null //todo: parse this properly
            };
        }

        private string GoogleGrpcMessageTypeToMessageType(FieldDescriptor field)
        {
            if (GoogleGrpcTypeToGrpcType(field) == GrpcType.__message)
            {
                return field.MessageType.Name;
            }

            return null;
        }


        public static GrpcType GoogleGrpcTypeToGrpcType(FieldDescriptor field)
        {
            string messageType = null;
            try
            {
                messageType = field.MessageType.Name;
            }
            catch (InvalidOperationException)
            {
                //swash these
            }

            var fieldType = field.FieldType;
            if (messageType == "Decimal64Value")
                return GrpcType.__company_Decimal64Value;
            else if (messageType == "StringValue")
                return GrpcType.__google_protobuf_StringValue;
            else if (messageType == "Timestamp")
                return GrpcType.__google_protobuf_Timestamp;
            else if (messageType == "Int32Value")
                return GrpcType.__google_protobuf_Int32Value;
            else if (fieldType == FieldType.Int64)
                return GrpcType.__int64;
            if (fieldType == FieldType.Bytes)
                return GrpcType.__bytes;
            if (fieldType == FieldType.Bool)
                return GrpcType.__bool;


            if (fieldType == FieldType.Float)
                return GrpcType.__float;
            if (fieldType == FieldType.Int32)
                return GrpcType.__int32;
            if (fieldType == FieldType.String)
                return GrpcType.__string;
            if (fieldType == FieldType.Double)
                return GrpcType.__double;
            if (field.IsMap)
            {
                return GrpcType.__map;
            }
            if (field.FieldType == FieldType.Message)
                return GrpcType.__message;
            if (field.FieldType == FieldType.Enum)
                return GrpcType.__enum;
            throw new NotImplementedException(
                $"No conversion implemented for google grpc type {fieldType} to Grpc type");
        }

        private string GenerateCodeFromProto(string protoFileContent)
        {
            if (string.IsNullOrEmpty(protoFileContent))
            {
                throw new ApplicationException("Proto File content cannot be empty");
            }

            var tempFile0 = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString().Replace("-", "")}");
            var tempFile = Path.Combine(tempFile0, "proto");

            var tempFile2 = Path.Combine(tempFile, $"{TempFilePrefix}.proto");

            Directory.CreateDirectory(Path.GetDirectoryName(tempFile2));
            File.WriteAllText(tempFile2, protoFileContent);
            var command = new StringBuilder();
            //command.AppendLine("cd /d %~dp0");
            command.AppendLine($"cd {tempFile0}");

           
            var versionGrpcTools = _configuration.GetValue<string>("Grpc_Tools_NugetVersion");
            var versionGoogleProtobuf = _configuration.GetValue<string>("Google_Protobuf_NugetVersion");
            if (versionGoogleProtobuf == null)
            {
                throw new ApplicationException("Google_Protobuf_NugetVersion not set");
            }

            command.AppendLine($@"SET GRPC_TOOLS=%USERPROFILE%\.nuget\packages\Grpc.Tools\{versionGrpcTools}\tools\windows_x64\");
            command.AppendLine($@"SET PROTOBUF_TOOLS=%USERPROFILE%\.nuget\packages\Google.Protobuf.Tools\{versionGoogleProtobuf}\tools");
            command.AppendLine(@"SET IN=Proto");
            command.AppendLine(@"SET OUTPUT=Proto");

            command.AppendLine(
                @"FOR /f ""tokens=*"" %%G IN ('dir %IN% /b ^| findstr proto') DO %GRPC_TOOLS%\protoc -I.\%IN%\;%PROTOBUF_TOOLS% --csharp_out %OUTPUT% %IN%/%%G --grpc_out %OUTPUT% --plugin=protoc-gen-grpc=%GRPC_TOOLS%\grpc_csharp_plugin.exe");

            var output = CommandProcessor.ExecuteCommand(command.ToString(), Path.GetDirectoryName(tempFile), true);
            
            if (Directory.GetFiles(Path.GetDirectoryName( tempFile2), "*.cs").Length == 0)
            {
                throw new ProtoCompileException(output);
            }
            return tempFile;
        }

       

        private Dictionary<string, FileDescriptor> GetFileDescriptorsFromAssembly(Assembly assembly)
        {
            //Type program = assembly.GetType("First.Program");
            //MethodInfo main = program.GetMethod("Main");

            var descriptors = new Dictionary<string, FileDescriptor>();
            
            assembly.GetTypes()
                //.Where(p => typeof(IReflection).IsAssignableFrom(p) && !p.IsInterface)
                .ToList()
                .ForEach(type =>
                {
                    try
                    {
                        var pi = type.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
                        if (pi != null)
                        {
                            var value = pi.GetValue(null);
                            if (value is FileDescriptor)
                            {
                                var csharpNameSpace = type.Namespace;
                                descriptors.Add(csharpNameSpace, (FileDescriptor) value);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            return descriptors;
        }

        
        private string GetGeneratedCode(string codePath)
        {
            var code = new StringBuilder();
            var codeFiles = Directory.EnumerateFiles(codePath, $"{TempFilePrefix}.cs").ToList();
            foreach (var file in codeFiles)
                code.Append(File.ReadAllText(file));
            return code.ToString();
        }

        private static Assembly CompileCodeToAssembly(string code)
        {
            return GenerateCode(code);
            
        }

        private static Assembly GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

        
        
        var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Google.Protobuf.ByteString).Assembly.Location),
                 MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location)
            };
            using (var ms = new MemoryStream())
            {
                
                var compilation = CSharpCompilation.Create(@"Hello.dll",
                    new[] {parsedSyntaxTree},
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Debug,
                        assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    var sb = new StringBuilder();
                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());

                        sb.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}" );
                    }
                    throw new InvalidOperationException(sb.ToString());

                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());
                    return assembly;
                }

            }

            return null;
        }
    }
}