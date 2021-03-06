using System.Collections.Generic;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass3.gRPC
{
    public class SProtoFileToProtoFileConverter : ISProtoFileToProtoFileConverter
    {
        #region Methods

        public string Convert(CProtoFile protoFile)
        {
            var codeWriter = new CodeWriter();

            codeWriter.WriteLine($@"syntax = ""{protoFile.ProtoSyntax}"";");
            codeWriter.WriteLine(string.Empty);
            if (protoFile.Package != null)
                foreach (var package in protoFile.Package)
                    codeWriter.WriteLine($@"package {package};");
            codeWriter.WriteLine();

            foreach (var import in protoFile.Import)
                codeWriter.WriteLine($@"import ""{import}"";");

            codeWriter.WriteLine();

            //todo: remove hard coding
            codeWriter.WriteLine(@"extend google.protobuf.FileOptions {");
            codeWriter.WriteLine(@"     string version = 50000;");
            codeWriter.WriteLine(@"}");

            foreach (var option in protoFile.Option)
                codeWriter.WriteLine($"option {option}");

            codeWriter.WriteLine();

            foreach (var service in protoFile.ProtoService)
            {
                codeWriter.WriteLine($"service {service.ServiceName} {{");
                codeWriter.Indent();
                foreach (var rpc in service.Rpc)
                {
                    codeWriter.WriteLine();
                    if (!string.IsNullOrEmpty(rpc.RpcDescription) && !string.IsNullOrWhiteSpace(rpc.RpcDescription))
                        codeWriter.WriteLine($@"/* {rpc.RpcDescription} */");
                    codeWriter.Write($"rpc {rpc.RpcName} ({rpc.Request.MessageName}) ");
                    codeWriter.WriteLine($"returns ({rpc.Response.MessageName});");
                }
                codeWriter.Unindent();
                codeWriter.WriteLine("}");
            }
            codeWriter.WriteLine();

            var messages = new List<CProtoMessage>();
            messages.AddRange(protoFile.ProtoMessage);
            foreach (var service in protoFile.ProtoService)
            foreach (var rpc in service.Rpc)
            {
                messages.Add(rpc.Request);
                messages.Add(rpc.Response);
            }

            foreach (var message in messages)
            {
                if (message.IsExternal)
                {
                    continue;
                }
                codeWriter.WriteLine($"message {message.MessageName} {{");
                codeWriter.Indent();

                var position = 1;
                foreach (var field in message.ProtoField)
                {
                    if (field.Repeated)
                        codeWriter.Write("repeated ");

                    var fieldType = string.Empty;
                    if (field.FieldType == GrpcType.__message)
                        fieldType = field.MessageType;
                    else if (field.FieldType == GrpcType.__enum)
                        fieldType = field.EnumType;
                    else if (field.FieldType == GrpcType.__map)
                        fieldType = field.MapType;
                    else
                        fieldType = field.FieldType.ToString().Replace("__", "").Replace("_", ".");
                    codeWriter.WriteLine($"{fieldType} {field.FieldNameGrpc} = {position}; // {field.Comment}");
                    position++;
                }

                codeWriter.Unindent();
                codeWriter.WriteLine("}");
            }
            codeWriter.WriteLine(string.Empty);

            foreach (var enumItem in protoFile.ProtoEnum)
            {
                codeWriter.WriteLine($"enum {enumItem.EnumName} {{");
                codeWriter.Indent();

                var position = 1;
                foreach (var enumItemValue in enumItem.EnumValue)
                {
                    codeWriter.WriteLine($"{enumItemValue.EnumValueName} = {enumItemValue.EnumValueNumber}; ");
                    position++;
                }

                codeWriter.Unindent();
                codeWriter.WriteLine("}");
            }
            codeWriter.WriteLine(string.Empty);
            return codeWriter.ToString();
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}