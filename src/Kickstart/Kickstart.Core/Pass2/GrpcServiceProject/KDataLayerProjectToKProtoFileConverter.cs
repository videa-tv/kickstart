using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.DataLayerProject.Table;
using Kickstart.Utility;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public class KDataLayerProjectToKProtoFileConverter : IKDataLayerProjectToKProtoFileConverter
    {
        private KDataStoreProject _databaseProject;

        public IList<KProtoFile> Convert(KDataStoreProject databaseProject, KDataLayerProject dataLayerProject ,
            KGrpcProject grpcKProject)
        {
            var protoFiles = new List<KProtoFile>();

            _databaseProject = databaseProject;
            foreach (var dataLayerInter in dataLayerProject.DataServiceInterface)
            {
                //var messagesToAddToBulkRpc = new List<SProtoMessage>();
                var kProtoFile = new KProtoFile { ProtoFileName = grpcKProject.ProjectName };
                kProtoFile.GeneratedProtoFile = BuildProtoFile(grpcKProject, kProtoFile, 
                    $@"{grpcKProject.CompanyName}.{grpcKProject.ProjectName}{grpcKProject.NamespaceSuffix}.{grpcKProject.ProjectSuffix}.Proto.Types");
                protoFiles.Add(kProtoFile);

                foreach (var method in dataLayerInter.Method)
                {
                    
                    var protoFile = kProtoFile.GeneratedProtoFile;
                    var protoService = protoFile.ProtoService.FirstOrDefault();
                    var storedProcedure = method.DerivedFrom as CStoredProcedure;

                    var storedProcedureList = method.DerivedFrom as CStoredProcList;
                    if (storedProcedure != null)
                    {
                        if (!storedProcedure.KickstartApi)
                            continue;
                        /*
                            if (!grpcKProject.KickstartCRUD)
                            {
                                //filter out anything that is CRUD
                                if (storedProcedure.DataOperationIs.HasFlag(SOperationIs.CRUD))
                                {
                                    continue;
                                }
                        
                            }*/
                        var rpc = BuildRpcFromStoredProc(protoFile, protoService, method, storedProcedure);

                        //if using Proto first, Rpc may already exist
                        if (!protoService.Rpc.Any(r => r.RpcName == rpc.RpcName))
                        {
                            protoService.Rpc.Add(rpc);
                        }
                    }
                    else if (storedProcedureList != null && storedProcedureList.List.Any())
                    {
                        var rpc = BuildBulkRpcFromStoredProcList(protoFile, protoService, method, storedProcedureList);
                        protoService.Rpc.Add(rpc);
                    }
                    
                    else if (method.MethodIs.HasFlag(COperationIs.Bulk))
                    {
                        // if (addBulkRpc)
                        if (false)
                        {
                            /*
                            var rpc = new SProtoRpc(protoService)
                            {
                                RpcName = $"{method.MethodName}",
                                OperationIs = SOperationIs.Bulk | SOperationIs.Add | SOperationIs.Update | SOperationIs.Delete
                            };

                            var request = new SProtoMessage
                            {
                                IsRequest = true,
                                MessageName = $"{rpc.RpcName}Request"

                            };

                            rpc.Request = request;
                            foreach (var message in messagesToAddToBulkRpc)
                            {
                                request.ProtoField.Add(new SProtoMessageField(null) { IsScalar = false, Repeated = true, MessageType = message.MessageName, FieldName = $"{message.MessageName}" });
                            }

                            var response = new SProtoMessage
                            {
                                IsResponse = true,
                                MessageName = $"{rpc.RpcName}Response"
                            };
                            rpc.Response = response;


                            protoService.Rpc.Add(rpc);*/
                        } 
                    }
                }
            }

            return protoFiles;
        }

        /*
        private KProtoFile FindKProtoFile(KGrpcProject grpcKProject, CMethod method)
        {
            KProtoFile kProtoFile = null;

            foreach (var mPf in grpcKProject.ProtoFile)
            {
                foreach (var mRpc in mPf.Rpc)
                    if (mRpc.RpcName.ToLower() == method.MethodName.ToLower())
                    {
                        kProtoFile = mPf;
                        break;
                    }
                if (kProtoFile != null)
                    break;
            }

            if (kProtoFile == null)
            {
                kProtoFile = grpcKProject.ProtoFile.FirstOrDefault();
                if (kProtoFile == null)
                {
                    kProtoFile = new KProtoFile {ProtoFileName = grpcKProject.ProjectName};
                    grpcKProject.ProtoFile.Add(kProtoFile);
                }
            }
            if (kProtoFile.GeneratedProtoFile == null)
            {
                var protoNamespace = kProtoFile.CSharpNamespace; //
                if (string.IsNullOrEmpty(protoNamespace))
                    protoNamespace =
                        $@"{grpcKProject.CompanyName}.{grpcKProject.ProjectName}{grpcKProject.NamespaceSuffix}.{
                                grpcKProject.ProjectSuffix
                            }.Proto.Types";
                kProtoFile.GeneratedProtoFile = BuildProtoFile(grpcKProject, kProtoFile, protoNamespace);
            }


            return kProtoFile;
        }*/

        private CProtoFile BuildProtoFile(KGrpcProject grpcKProject, KProtoFile kProtoFile, string protoNamespace)
        {
            var protoFile = new CProtoFile();

            protoFile.Import.Add("google/protobuf/timestamp.proto");
            protoFile.Import.Add("google/protobuf/duration.proto");
            protoFile.Import.Add("google/protobuf/descriptor.proto");

            protoFile.CSharpNamespace = protoNamespace;

            protoFile.Option.Add($@"csharp_namespace = ""{protoNamespace}"";");
            protoFile.Option.Add($@"(version) = ""1.0.0"";");

            var protoService = new CProtoService(protoFile)
            {
                ServiceName = $@"{kProtoFile.ProtoFileName}Service"
            };
            protoFile.ProtoService.Add(protoService);

            return protoFile;
        }

        private CProtoRpc BuildBulkRpcFromStoredProcList(CProtoFile protoFile, CProtoService protoService,
            CMethod method, CStoredProcList storedProcedureList)
        {
            var rpc = new CProtoRpc(protoService)
            {
                RpcName = method.MethodName,
                //DomainModelName = "Junk", // storedProcedure.ResultSetName,,
                DerivedFrom = storedProcedureList

            };
            rpc.Request = new CProtoMessage (rpc)
            {
                IsRequest = true,
                MessageName = $"{method.MethodName}Request"
            };
            rpc.Response = new CProtoMessage(rpc)
            {
                IsResponse = true,
                MessageName = $"{method.MethodName}Response"
            };

            
            var requestMessage = rpc.Request;
            foreach (var storedProcedure in storedProcedureList.List)
            {
                if (!string.IsNullOrEmpty(storedProcedure.ParameterSetName))
                {
                    rpc.Request.ProtoField.Add(new CProtoMessageField(storedProcedure)
                    {
                        IsScalar = false,
                        MessageType = storedProcedure.ParameterSetName,
                        FieldName = storedProcedure.ParameterSetName,
                        Repeated = true
                    });

                    requestMessage = new CProtoMessage(rpc)
                    {
                        IsRequest = true,
                        MessageName = storedProcedure.ParameterSetName
                    };
                    if (!protoFile.ProtoMessage.Exists(pm => pm.MessageName == requestMessage.MessageName))
                        protoFile.ProtoMessage.Add(requestMessage);
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
                /*
                var responseMessage = rpc.Response;
                if (!string.IsNullOrEmpty(storedProcedure.ResultSetName))
                {
                    rpc.Response.ProtoField.Add(new SProtoMessageField(null)
                    {
                        IsScalar = false,
                        Repeated = true,
                        MessageType = storedProcedure.ResultSetName,
                        FieldName = storedProcedure.ResultSetName
                    });

                    responseMessage = new SProtoMessage
                    {
                        IsResponse = true,
                        MessageName = storedProcedure.ResultSetName
                    };
                    if (!protoFile.ProtoMessage.Exists(pm => pm.MessageName == responseMessage.MessageName))
                    {
                        protoFile.ProtoMessage.Add(responseMessage);

                    }

                }
                foreach (var resultColumn in storedProcedure.ResultSet)
                {
                    var field = new SProtoMessageField(resultColumn)
                    {
                        FieldName = resultColumn.ColumnName,
                        FieldType = SqlMapper.SqlDbTypeToGrpcType(resultColumn.ColumnSqlDbType)
                    };

                    responseMessage.ProtoField.Add(field);
                }*/
            }
            return rpc;
        }

        private CProtoRpc BuildRpcFromStoredProc(CProtoFile protoFile, CProtoService protoService, CMethod method,
            CStoredProcedure storedProcedure)
        {
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
            rpc.Response = new CProtoMessage (rpc)
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
                    protoFile.ProtoMessage.Add(requestMessage);
            }
            foreach (var parameter in storedProcedure.Parameter)
            {
                var field = new CProtoMessageField(parameter)
                {
                    FieldName = parameter.ParameterName //.SourceColumn.ColumnName,
                };

                if (!parameter.ParameterTypeIsUserDefined)
                {
                    var sqlType = SqlMapper.ParseValueAsSqlDbType(parameter.ParameterTypeRaw);
                    field.FieldType = SqlMapper.SqlDbTypeToGrpcType(sqlType);
                }
                else
                {
                    //todo: property handle user defined sql types (tables)
                    //lookup table type
                    //for now, use the data type of the first column, assumes single column table
                    var tableType = FindTableType(parameter.ParameterTypeRaw);
                    var converter = new CTableTypeToCClassConverter();
                    var @class = converter.Convert(tableType.GeneratedTableType);
                    field.FieldType = GrpcType.__string;
                    field.IsScalar = false;
                    field.Repeated = true;
                    field.MessageType = $@"{@class.ClassName}";

                    if (!protoFile.ProtoMessage.Exists(pm => pm.MessageName == field.MessageType))
                    {
                        //create a message
                        var tableTypeDerivedMessage = new CProtoMessage (rpc)
                        {
                            MessageName = field.MessageType
                        };
                        foreach (var property in @class.Property)
                        {
                            var field2 = new CProtoMessageField(property)
                            {
                                FieldName = property.PropertyName,
                                FieldType = SqlMapper.ClrTypeToGrpcType(SqlMapper.ClrTypeAliasToClrType(property.Type))
                            };

                            tableTypeDerivedMessage.ProtoField.Add(field2);
                        }
                        protoFile.ProtoMessage.Add(tableTypeDerivedMessage);
                    }
                }

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
            return rpc;
        }

        private KTableType FindTableType(string parameterTypeRaw)
        {
            return _databaseProject.TableType.FirstOrDefault(t =>
                t.GeneratedTableType.TableName == parameterTypeRaw);
        }
    }
}