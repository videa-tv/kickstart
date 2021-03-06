using System.Collections.Generic;
using System.Data;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public class CStoredProcedureToCProtoFileConverter
    {
        #region Methods

        public CProtoFile Convert(List<CStoredProcedure> storedProcedures, bool addCRUD = false, bool addBulkRpc = true)
        {
            var protoFile = new CProtoFile();

            protoFile.Import.Add("google/protobuf/timestamp.proto");
            protoFile.Import.Add("google/protobuf/descriptor.proto");

            //protoFile.Import.Add("google/protobuf/duration.proto");
            protoFile.CSharpNamespace = NamespaceName;

            protoFile.Option.Add($@"csharp_namespace = ""{NamespaceName}"";");

            var messagesToAddToBulkRpc = new List<CProtoMessage>();
            var protoService = new CProtoService(protoFile)
            {
                ServiceName = ServiceName
            };

            protoFile.ProtoService.Add(protoService);

            foreach (var storedProcedure in storedProcedures)
            {
                if (!addCRUD)
                    if (storedProcedure.DataOperationIs.HasFlag(COperationIs.CRUD))
                        continue;

                var rpc = new CProtoRpc(protoService)
                {
                    RpcName = storedProcedure.StoredProcedureName,
                    RpcDescription = storedProcedure.StoredProcedureDescription,
                    //DomainModelName = storedProcedure.ResultSetName,
                    DerivedFrom = storedProcedure
                };
                rpc.Request = new CProtoMessage (rpc)
                {
                    IsRequest = true,
                    MessageName = $"{storedProcedure.StoredProcedureName}Request"
                };
                rpc.Response = new CProtoMessage(rpc)
                {
                    IsResponse = true,
                    MessageName = $"{storedProcedure.StoredProcedureName}Response"
                };
                    
                var requestMessage = rpc.Request;
                if (!string.IsNullOrEmpty(storedProcedure.ParameterSetName))
                {
                    rpc.Request.ProtoField.Add(new CProtoMessageField(storedProcedure)
                    {
                        IsScalar = false,
                        MessageType = storedProcedure.ParameterSetName,
                        FieldName = storedProcedure.ParameterSetName
                    });

                    requestMessage = new CProtoMessage(rpc)
                    {
                        IsRequest = true,
                        MessageName = storedProcedure.ParameterSetName
                    };
                    if (!protoFile.ProtoMessage.Exists(pm => pm.MessageName == requestMessage.MessageName))
                    {
                        protoFile.ProtoMessage.Add(requestMessage);

                        if (addBulkRpc)
                            messagesToAddToBulkRpc.Add(requestMessage);
                    }
                }
                foreach (var parameter in storedProcedure.Parameter)
                {
                    var sqlType = SqlDbType.VarChar;
                    if (!parameter.ParameterTypeIsUserDefined)
                        sqlType = SqlMapper.ParseValueAsSqlDbType(parameter.ParameterTypeRaw);

                    var field = new CProtoMessageField(parameter)
                    {
                        FieldName = parameter.ParameterName, //.SourceColumn.ColumnName,
                        FieldType = SqlMapper.SqlDbTypeToGrpcType(sqlType)
                    };

                    if (parameter.ParameterTypeIsUserDefined)
                        field.Repeated = true;
                    requestMessage.ProtoField.Add(field);
                }

                var responseMessage = rpc.Response;
                if (!string.IsNullOrEmpty(storedProcedure.ResultSetName))
                {
                    rpc.Response.ProtoField.Add(new CProtoMessageField(null)
                    {
                        IsScalar = false,
                        Repeated = true,
                        MessageType = storedProcedure.ResultSetName,
                        FieldName = storedProcedure.ResultSetName
                    });

                    responseMessage = new CProtoMessage (rpc)
                    {
                        IsResponse = true,
                        MessageName = storedProcedure.ResultSetName
                    };
                    if (!protoFile.ProtoMessage.Exists(pm => pm.MessageName == responseMessage.MessageName))
                        protoFile.ProtoMessage.Add(responseMessage);
                }
                foreach (var resultColumn in storedProcedure.ResultSet)
                {
                    var field = new CProtoMessageField(resultColumn)
                    {
                        FieldName = resultColumn.ColumnName,
                        FieldType = SqlMapper.SqlDbTypeToGrpcType(resultColumn.ColumnSqlDbType)
                    };

                    responseMessage.ProtoField.Add(field);
                }

                protoService.Rpc.Add(rpc);
            }
            /*
            if (addBulkRpc)
            {
                var rpc = new SProtoRpc(protoService)
                {
                    RpcName = $"{BulkStoreRpcName}",
                    OperationIs = SOperationIs.Bulk | SOperationIs.Add | SOperationIs.Update
                };

                var request = new SProtoMessage
                {
                    IsRequest = true,
                    MessageName = $"{rpc.RpcName}Request"
                    
                };

                rpc.Request = request;
                foreach (var message in messagesToAddToBulkRpc)
                {
                    request.ProtoField.Add(new SProtoMessageField (null) { IsScalar = false, Repeated = true, MessageType = message.MessageName, FieldName = $"{message.MessageName}" });
                }

                var response = new SProtoMessage
                {
                    IsResponse = true,
                    MessageName = $"{rpc.RpcName}Response"
                };
                rpc.Response = response;


                protoService.Rpc.Add(rpc);
            }*/

            return protoFile;
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        public string NamespaceName { get; set; }

        public string ServiceName { get; set; }
        //public string BulkStoreRpcName { get; set; } = "BulkStore";

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}