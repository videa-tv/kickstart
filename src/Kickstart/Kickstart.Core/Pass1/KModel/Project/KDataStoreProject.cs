using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DocumentFormat.OpenXml.Math;
using Google.Protobuf.WellKnownTypes;
using Kickstart.Pass1.KModel.Data;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.SqlServer;
using Kickstart.Utility;

namespace Kickstart.Pass1.KModel
{
    public static class StringExtensionMethods
    {
        public static string ReplaceAtStart(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0 || pos > 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }

    public partial class KDataStoreProject : KProject
    {
        public KDataStoreProject()
        {
            ProjectIs = CProjectIs.DataBase;
            ProjectSuffix = "Db";
        }
        public DataStoreTypes DataStoreType { get; set; } = DataStoreTypes.SqlServer;

        public bool KickstartCRUDStoredProcedures { get; set; } = true;
        public bool KickstartTableSeedScripts { get; set; } = true;

        public string DbSuffix { get; set; }
        public string DefaultSchemaName { get; set; }

        internal void ClearSqlMeta()
        {
            OldTable.Clear();
            OldTable.AddRange(Table);

            Table.Clear();

            OldStoredProcedure.Clear();
            OldStoredProcedure.AddRange(StoredProcedure);
            StoredProcedure.Clear();

            OldTableType.Clear();
            OldTableType.AddRange(TableType);
            TableType.Clear();

            Query.Clear();
            MockView.Clear();
            View.Clear();
        }

        public bool KickstartAuditTables { get; set; } = true;

        public bool KickstartLoadDateUtc { get; set; } = true;
        public bool KickstartModifiedBy { get; set; } = false;
        public bool KickstartModifiedByAppUser { get; set; } = false;

        public void InferSqlFromProto(KProtoFile kProtoFile)
        {
            SqlTableText = InferTables(kProtoFile);
            SqlTableTypeText = InferTableTypes(kProtoFile);
            SqlStoredProcedureText = InferStoredProcedures(kProtoFile);

            SqlViewText = InferViews(kProtoFile);
            InferQueries(kProtoFile);
        }

        private string InferTables(KProtoFile kProtoFile)
        {
            var tables = new List<CTable>();
            //generate Sql text, it will turn back into a KTable later
            var codeWriter = new CodeWriter();
            foreach (var rpc in kProtoFile.GeneratedProtoFile.GetRpcListThatGet())
            {
                var request = rpc.GetInnerMessageOrRequest();

                InferTable(tables,rpc,request, null );

                var response = rpc.GetInnerMessageOrResponse();
                InferTable(tables, rpc, response, null);

            }

            foreach (var rpc in kProtoFile.GeneratedProtoFile.GetRpcListThatSet())
            {
                var request = rpc.GetInnerMessageOrRequest();

                InferTable(tables, rpc, request, null);

                var response = rpc.GetInnerMessageOrResponse();
                InferTable(tables, rpc, response, null);

            }

            var converter = new CTableToSqlServerTableConverter();

            foreach (var table in tables)
            { 
                
                codeWriter.WriteLine(converter.Convert(table));
                codeWriter.WriteLine("GO");
                codeWriter.WriteLine();
                this.Table.Add(new KTable { DerivedFrom = table.DerivedFrom, Schema = new CSchema { SchemaName = table.Schema.SchemaName }, TableName = table.TableName, GeneratedTable = table });
            }
            return codeWriter.ToString();

        }

        private void InferTable(IList<CTable> tables, CProtoRpc rpc, CProtoMessage messageIn, CTable parentTable)
        {
            var table = new CTable(DataStoreTypes.SqlServer);

            table.Schema = InferSchema(rpc.ProtoService);
            table.TableName = InferTableName(messageIn);
            table.DerivedFrom = messageIn;
            //add PK, Identity
            table.Column.Add(new CColumn(table) { ColumnName = $"{table.TableName}Id", IsNullable = false, ColumnTypeRaw = "bigint", ColumnType = System.Data.DbType.Int64, IsPrimaryKey = true, IsIdentity = true });

            if (parentTable != null)
            {
                var fk = parentTable.Column.FirstOrDefault(c => c.IsPrimaryKey);
                if (fk != null)
                {
                    var fkColumn = new CColumn(table)
                    {
                        ColumnName =  fk.ColumnName,
                        IsNullable =  fk.IsNullable,
                        ColumnType =  fk.ColumnType,
                        ColumnTypeRaw = fk.ColumnTypeRaw,
                        IsIdentity =  false
                    };
                    fkColumn.ForeignKeyColumn.Add(fk);

                    table.Column.Add(fkColumn);
                }
            }

            foreach (var p in messageIn.ProtoField)
            {
                var parameter = p;
                if (parameter.FieldType == GrpcType.__message)
                {
                    //if the inner message is just a single scalar field, lets use that
                    var m = rpc.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm => pm.MessageName == parameter.MessageType);
                    if (m.ProtoField.Count == 1 && !parameter.Repeated)
                    {
                        parameter = m.ProtoField.First();
                    }
                    else if (m.ProtoField.Count > 1)
                    {
                       
                        continue;
                    }
                    else
                    {
                        continue;
                        
                    }

                }
                var columns = InferColumns(table, rpc, parameter);

                foreach (var col in columns)
                {
                    if (!table.ColumnExists(col))
                    {
                        table.Column.Add(col);
                    }
                }
                
            }

          
            var existingTable = tables.FirstOrDefault(t => t.TableName == table.TableName);

            if (existingTable != null)
            {
                //merge the tables
                foreach (var col in table.Column)
                {
                    if (!existingTable.ColumnExists(col))
                    {
                        existingTable.Column.Add(col);
                    }
                }

            }
            else
                tables.Add(table);

            //now that tables has been added, add children
            //this will attempt to make sure they are in correct order for FK constraints
            foreach (var p in messageIn.ProtoField)
            {
               
                if (p.FieldType == GrpcType.__message && p.Repeated)
                {
                    //if the inner message is just a single scalar field, lets use that
                    var m = rpc.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm =>
                        pm.MessageName == p.MessageType);
                    
                    // if (m.ProtoField.Count > 1) //its repeated, so field count doesn't matter
                    {
                        //recurse
                        InferTable(tables, rpc, m, table);
                      
                    }

                }
            }
        }

        private string InferTableTypes(KProtoFile kProtoFile)
        {
            var tableTypes = new List<CTableType>();
            //generate Sql text, it will turn back into a KTable later
            var codeWriter = new CodeWriter();
            foreach (var rpc in kProtoFile.GeneratedProtoFile.GetRpcListThatSet())
            {
                var request = rpc.GetInnerMessageOrRequest();

                InferTableType(tableTypes, rpc, request);
 
            }

            foreach (var rpc in kProtoFile.GeneratedProtoFile.GetRpcListThatGet())
            {
                var response = rpc.GetInnerMessageOrResponse();

                InferTableType(tableTypes, rpc, response);

            }


            var converter = new CTableTypeToSqlServerTableTypeConverter();

            foreach (var tableType in tableTypes)
            {

                codeWriter.WriteLine(converter.Convert(tableType));
                codeWriter.WriteLine("GO");
                codeWriter.WriteLine();
                this.TableType.Add(new KTableType() {
                    DerivedFrom = tableType.DerivedFrom,
                    Schema = tableType.Schema.SchemaName ,
                    TableTypeName = tableType.TableName, GeneratedTableType = tableType
                });
            }

           return codeWriter.ToString();

        }

        private void InferTableType(IList<CTableType> tableTypes, CProtoRpc rpc, CProtoMessage messageIn)
        {
            var tableType = new CTableType(DataStoreTypes.SqlServer);

            tableType.Schema = InferSchema(rpc.ProtoService);
            tableType.TableName ="tt"+ InferTableName(messageIn);
            tableType.DerivedFrom = messageIn;
            //add PK
            tableType.Column.Add(new CColumn(tableType) { ColumnName = $"{tableType.TableName}Id", IsNullable = false, ColumnTypeRaw = "bigint", ColumnType = System.Data.DbType.Int64, IsPrimaryKey = true, IsIdentity = false });

            foreach (var p in messageIn.ProtoField)
            {
                var parameter = p;
                if (parameter.FieldType == GrpcType.__message)
                {
                    //if the inner message is just a single scalar field, lets use that
                    var m = rpc.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm => pm.MessageName == parameter.MessageType);
                    if (m.ProtoField.Count == 1 && !parameter.Repeated)
                    {
                        parameter = m.ProtoField.First();
                    }
                    else if (m.ProtoField.Count > 1)
                    {
                        continue;
                    }
                    else
                    {
                        continue;
                    }

                }
                var columns = InferColumns(tableType, rpc, parameter);

                foreach (var col in columns)
                {
                    if (!tableType.ColumnExists(col))
                    {
                        tableType.Column.Add(col);
                    }
                }

                
               
            }


            var existingTable = tableTypes.FirstOrDefault(t => t.TableName == tableType.TableName);

            if (existingTable != null)
            {
                //merge the tableTypes
                foreach (var col in tableType.Column)
                {
                    if (!existingTable.ColumnExists(col))
                    {
                        existingTable.Column.Add(col);
                    }
                }

            }
            else
                tableTypes.Add(tableType);

            //now that tables has been added, add children
            //this will attempt to make sure they are in correct order for FK constraints
            foreach (var p in messageIn.ProtoField)
            {
                if (p.FieldType == GrpcType.__message && p.Repeated)
                {
                    //if the inner message is just a single scalar field, lets use that
                    var m = rpc.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm =>
                        pm.MessageName == p.MessageType);

                    // if (m.ProtoField.Count > 1) //its repeated, so field count doesn't matter
                    {
                        //recurse
                        InferTableType(tableTypes, rpc, m);

                    }

                }
            }
        }
      
        private IEnumerable<CColumn> InferColumns(CTable table, CProtoRpc rpc, CProtoMessageField protoField)
        {
            var columns = new List<CColumn>();
            DbType columnType1 = DbType.AnsiString;
            string columnType1Raw = null;

            var field = protoField;
            if (field.IsMap)
            {
                //todo: don't hard code ast <string, bool>

                //loop thru the 2 fields in the map and add columns
                columnType1 =
                    SqlMapper.SqlDbTypeToDbType(SqlMapper.GrpcTypeToSqlDbType(Pass2.CModel.GrpcType.__string));
                var col1 = new CColumn(table)
                {
                    ColumnName = $"{field.FieldName}{columnType1.ToString()}",
                    ColumnType = columnType1,
                    ColumnTypeRaw = "varchar"
                };
                columns.Add((col1));


                var columnType2 =
                    SqlMapper.SqlDbTypeToDbType(SqlMapper.GrpcTypeToSqlDbType(Pass2.CModel.GrpcType.__bool));
                var col2 = new CColumn(table)
                {
                    ColumnName = $"{field.FieldName}{columnType2.ToString()}",
                    ColumnType = columnType2,
                    ColumnTypeRaw = "bit"
                };
                columns.Add((col2));

            }
            else
            {
                if (field.IsTimestampMessage)
                {
                    columnType1 = DbType.DateTime2;
                    columnType1Raw = DbType.DateTime2.ToString();
                }
                else
                {
                    if (field.FieldType == GrpcType.__message)
                    {
                        //if the inner message is just a single scalar field, lets use that
                        var m = rpc.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm =>
                            pm.MessageName == field.MessageType);
                        if (m.ProtoField.Count == 1)
                        {
                            field = m.ProtoField.First();
                        }

                    }

                    columnType1 = SqlMapper.SqlDbTypeToDbType(SqlMapper.GrpcTypeToSqlDbType(field.FieldType));
                    columnType1Raw = SqlMapper.GrpcTypeToSqlDbType(field.FieldType).ToString();

                    if (field.FieldName.EndsWith("ExternalId"))
                    {
                        columnType1 = System.Data.DbType.AnsiStringFixedLength;
                        columnType1Raw = "char";
                    }
                }

                var col = new CColumn(table)
                {
                    ColumnName = field.FieldName,
                    ColumnType = columnType1,
                    ColumnTypeRaw = columnType1Raw
                };
                if (columnType1 == System.Data.DbType.AnsiStringFixedLength)
                {
                    if (col.ColumnName.EndsWith("ExternalId"))
                    {
                        col.ColumnLength = 32;
                    }
                    else
                    {
                        col.ColumnLength = 255;
                    }
                }
                else if (columnType1 == System.Data.DbType.AnsiString)
                {
                    col.ColumnLength = 255;
                }

                columns.Add(col);
            }

            return columns;
        }


        private static string InferTableName(CProtoMessage message)
        {
            var tableName = message.MessageName;
            
            tableName = tableName.ReplaceAtStart("GetAll", "");

            tableName = tableName.ReplaceAtStart("Get", "");
            tableName = tableName.ReplaceAtStart("Is", "");

            tableName = tableName.ReplaceAtStart("List", "");

            tableName = tableName.ReplaceAtStart("Add", "");
            tableName = tableName.ReplaceAtStart("Save", "");
            tableName = tableName.ReplaceAtStart("Approve", "");

            tableName = tableName.Replace("Find", "");

            tableName = tableName.Replace("Check", "");

            tableName = tableName.Replace("Read", "");

            tableName = tableName.Replace("Create", "");

            tableName = tableName.Replace("Queue", "");

            tableName = tableName.Replace("Dequeue", "");

            tableName = tableName.Replace("Delete", "");

            tableName = tableName.Replace("Response", "");

            tableName = tableName.Replace("Request", "");

            var indexOfBy = tableName.IndexOf("By", StringComparison.CurrentCulture);
            if (indexOfBy > -1)
            {
                tableName = tableName.Substring(0, indexOfBy);
            }

            var s = new Inflector.Inflector(CultureInfo.CurrentCulture);
            tableName = s.Singularize(tableName);
            return tableName;
        }

        private CSchema InferSchema(CProtoService cProtoService)
        {
            var schemaName = cProtoService.ServiceName;
            if (schemaName.EndsWith("Service"))
            {
                schemaName = schemaName.Substring(0, schemaName.LastIndexOf("Service")).ToLower();
            }

            return new CSchema { SchemaName = schemaName };
        }

        private void InferQueries(KProtoFile kProtoFile)
        {
            var codeWriter = new CodeWriter();
            foreach (var message in kProtoFile.GeneratedProtoFile.GetRepeatedMessagesUsedInAResponse())
            {
                if (message.IsExternal)
                {
                    continue;
                }
                if (message.IsRequest)
                {
                    continue;
                }
                if (message.HasFields)
                {
                    Query.Add(new KQuery { QueryName = $"{message.MessageName}Query" });
                }
            }
        }

        private string InferViews(KProtoFile kProtoFile)
        {
            var codeWriter = new CodeWriter();
            foreach (var message in kProtoFile.GeneratedProtoFile.GetRepeatedMessagesUsedInAResponse() )
            {
                if (message.IsExternal)
                {
                    continue;
                }
                if (message.IsRequest)
                {
                    continue;
                }
                if (message.HasFields)
                {
                    codeWriter.WriteLine($"CREATE VIEW [dbo].[{message.MessageName}View] AS");
                    codeWriter.WriteLine("SELECT");

                    codeWriter.Indent();
                    bool first = true;
                    foreach (var field in message.ProtoField)
                    {
                        if (!first)
                        {
                            codeWriter.Write(",");
                        }
                        codeWriter.WriteLine($"NULL AS '{field.FieldName}'");
                        first = false;
                    }
                    codeWriter.Unindent();
                    codeWriter.WriteLine();
                    codeWriter.WriteLine("GO");
                    codeWriter.WriteLine();
                }

            }
            return codeWriter.ToString();
        }

        private string InferStoredProcedures(KProtoFile kProtoFile)
        {

            var codeWriter = new CodeWriter();


            var convert = new CStoredProcedureToSSqlServerStoredProcedureConverter();
            foreach (var service in kProtoFile.GeneratedProtoFile.ProtoService)
            {
                foreach (var rpc in service.Rpc)
                {
                    var request = rpc.GetInnerMessageOrRequest();
                    //if  (!request.ProtoField.Any(f=>f.FieldType != GrpcType.__message))
                    //    continue;
                    
                    var storedProc = new CStoredProcedure(DataStoreTypes.Unknown)
                    {
                        DerivedFrom = request, // rpc,
                        Schema = InferSchema(service),
                        StoredProcedureName = rpc.RpcName,
                        ResultSetName = rpc.Response.MessageName + "Dto"//"ResultSet"
                    };

                    foreach (var pp in request.ProtoField)
                    {
                        var parameter = pp;
                        if (parameter.FieldType == GrpcType.__message)
                        {
                            //if the inner message is just a single scalar field, lets use that
                            var m = rpc.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm => pm.MessageName == parameter.MessageType);
                            if (m.ProtoField.Count == 1)
                            {
                                
                                parameter = m.ProtoField.First();
                                /*
                                if (parameter.FieldType == GrpcType.__message)
                                {
                                    m = rpc.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm =>
                                        pm.MessageName == parameter.MessageType);
                                    if (m.ProtoField.Count == 1)
                                    {
                                        parameter = m.ProtoField.First();

                                    }
                                    else if (m.ProtoField.Count > 1)
                                    {
                                        Debugger.Break();
                                        
                                    }
                                }*/
                            }
                            else if (m.ProtoField.Count > 1)
                            {
                                
                                /*
                                //todo: we need to pass table valued parameters to query
                                Debugger.Break();
                                parameter = m.ProtoField.First();
                               
                                if (parameter.FieldType == GrpcType.__message)
                                {
                                    m = rpc.ProtoService.ProtoFile.ProtoMessage.FirstOrDefault(pm =>
                                        pm.MessageName == parameter.MessageType);
                                    if (m.ProtoField.Count == 1)
                                    {
                                        parameter = m.ProtoField.First();

                                    }
                                    else if (m.ProtoField.Count > 1)
                                    {
                                        //todo: we need to pass table valued parameters to query
                                        Debugger.Break();
                                        parameter = m.ProtoField.First();
                                    
                                    }
                                }
                                */
                            
                            
                            }

                        }

                        var p = InferStoredProcedureParameter(parameter);
                        if (!storedProc.Parameter.Exists(p2=>p2.ParameterName == p.ParameterName ))
                            storedProc.Parameter.Add(p);

                    }

                    var tables = new List<CTable>();
                    var request2 = rpc.GetInnerMessageOrRequest();

                    InferTable(tables, rpc, request2, null);
                    var table = tables.First();

                    if (rpc.OperationIs.HasFlag(COperationIs.Get ) ||
                        rpc.OperationIs.HasFlag(COperationIs.Find) ||
                        rpc.OperationIs.HasFlag(COperationIs.List) ||
                        rpc.OperationIs.HasFlag(COperationIs.Read) ||
                        rpc.OperationIs.HasFlag(COperationIs.Dequeue) ||
                        rpc.OperationIs.HasFlag(COperationIs.Check)
                        )
                    {
                        storedProc.StoredProcedureBody = $@"SELECT {table.ColumnAsColumnList()} FROM {table.Schema.SchemaName.WrapReservedAndSnakeCase(DataStoreTypes.SqlServer, false)}.{table.TableName.WrapReservedAndSnakeCase(DataStoreTypes.SqlServer, false)}";
                    }
                    else if (rpc.OperationIs.HasFlag(COperationIs.Set) ||
                             rpc.OperationIs.HasFlag(COperationIs.Update) ||
                             rpc.OperationIs.HasFlag(COperationIs.Create))
                    {
                        storedProc.StoredProcedureBody = $@"PRINT 'todo: UPDATE {table.Schema.SchemaName}.{table.TableName} SET ....'";
                    }

                    codeWriter.WriteLine(convert.Convert(storedProc));
                    codeWriter.WriteLine();
                    codeWriter.WriteLine("GO");
                    codeWriter.WriteLine();

                    this.StoredProcedure.Add(new KStoredProcedure
                    {
                        StoredProcedureName = storedProc.StoredProcedureName,
                        Schema = storedProc.Schema.SchemaName,
                        ReturnsMultipleRows = storedProc.ReturnsMultipleRows,

                        ResultSetName = storedProc.ResultSetName,
                        ParameterSetName = storedProc.ParameterSetName,
                        DerivedFrom = storedProc.DerivedFrom,
                        GeneratedStoredProcedure = storedProc
                    });

                }
            }
            return codeWriter.ToString();

        }

        private static CStoredProcedureParameter InferStoredProcedureParameter(CProtoMessageField parameter)
        {
            DbType parameterType;
            string parameterTypeRaw;
            bool isUserDefined = false;
            if (parameter.IsTimestampMessage)
            {
                parameterType = DbType.DateTime2;
                parameterTypeRaw = DbType.DateTime2.ToString();
            }
            else if (parameter.FieldType == GrpcType.__message)
            {
                isUserDefined = true;
                parameterType = SqlMapper.SqlDbTypeToDbType(SqlMapper.GrpcTypeToSqlDbType(parameter.FieldType));
                parameterTypeRaw = $"tt{parameter.MessageType}"; //todo: look it up

            }
            else
            {
                parameterType = SqlMapper.SqlDbTypeToDbType(SqlMapper.GrpcTypeToSqlDbType(parameter.FieldType));
                parameterTypeRaw = SqlMapper.GrpcTypeToSqlRaw(parameter.FieldType);
            }

            var p = new CStoredProcedureParameter
            {
                ParameterName = parameter.FieldName,
                ParameterType = parameterType,

                ParameterTypeRaw = parameterTypeRaw,
                ParameterTypeIsUserDefined = isUserDefined,
                IsCollection = parameter.Repeated
            };

            if (p.ParameterType == System.Data.DbType.AnsiString)
            {
                p.ParameterLength = 255;
            }

            return p;
        }

        public bool KickstartModifiedDateUtc { get; set; } = true;
        public bool KickstartCreatedBy { get; set; } = false;
        public bool KickstartCreatedByAppUser { get; set; } = false;
        public bool KickstartCreatedDateUtc { get; set; } = true;
        public bool KickstartEffectiveDateUtc { get; set; } = true;
        public bool KickstartHashDiff { get; set; } = true;

        public string SqlViewFile { get; set; }

        public string SqlViewText { get; set; }
        
        public string SqlTableFile { get; set; }

        public string InferProjectName()
        {
            return Table.FirstOrDefault().Schema.SchemaName;
        }

        private string _sqlTableText;
        public string SqlTableText {
            get
            {
                return _sqlTableText;
            }
            set
            {
                _sqlTableText = value;
            }
        }

        public string MockSqlViewFile { get; set; }

        public string MockSqlViewText { get; set; }
        public string SqlTableTypeFile { get; set; }
        public string SqlTableTypeText { get; set; }

        public string SqlStoredProcedureFile { get; set; }
        public string SqlStoredProcedureText { get; set; }

        public List<KTable> OldTable { get; set; } = new List<KTable>();


        public List<KTable> Table { get; set; } = new List<KTable>();
        public List<KView> View { get; set; } = new List<KView>();
        public List<KFunction> Function { get; set; } = new List<KFunction>();

        public List<KView> MockView { get; set; } = new List<KView>();
        public List<KTableType> TableType { get; set; } = new List<KTableType>();

        public List<KStoredProcedure> OldStoredProcedure { get; private set; } = new List<KStoredProcedure>();
        public List<KTableType> OldTableType { get; private set; } = new List<KTableType>();

        public List<KStoredProcedure> StoredProcedure { get; set; } = new List<KStoredProcedure>();

        public List<KQuery> Query { get; set; } = new List<KQuery>();
        public bool GenerateStoredProcAsEmbeddedQuery { get; set; } = true;
        public bool ConvertToSnakeCase { get; set; }
        public string SqlFunctionText { get; set; }

        public void AddSeedData()
        {
            if (SeedDataService != null)
                SeedDataService.AddSeedData(MockView);
        }

        public override void ConfigureMetaData()
        {
            if (MetadataConfigService != null)
                MetadataConfigService.ConfigureMetaData(MockView, StoredProcedure, Table);
        }

        public override void ConfigureMetaData2()
        {
            foreach (var storedProc in StoredProcedure)
                //default the values, per stored proc, so individual stored proc values can be override in
                //ConfigureMetaData();

                if (storedProc.GeneratedStoredProcedure.DataOperationIs.HasFlag(COperationIs.CRUD))
                    storedProc.GeneratedStoredProcedure.KickstartApi = false;
                else
                    storedProc.GeneratedStoredProcedure.KickstartApi = true;
            if (MetadataConfigService != null)
                MetadataConfigService.ConfigureMetaData2(MockView, StoredProcedure, Table);
        }

        public List<CSchema> GetAllGeneratedSchemas()
        {
            var schemas = new List<CSchema>();

            foreach (var table in Table)
            {
                if (schemas.Exists(c=>c.SchemaName == table.GeneratedTable.Schema.SchemaName))
                {
                    continue;
                }
                schemas.Add(new CSchema { SchemaName = table.GeneratedTable.Schema.SchemaName });

            }
            foreach (var view in View)
            {
                //AddSchema(dataProject, $"{view.GeneratedView.Schema.SchemaName}");
                if (schemas.Exists(c => c.SchemaName == view.GeneratedView.Schema.SchemaName))
                {
                    continue;
                }
                schemas.Add(new CSchema { SchemaName = view.GeneratedView.Schema.SchemaName });

            }
            foreach (var storeProcedure in StoredProcedure)
            {
                if (schemas.Exists(c => c.SchemaName == storeProcedure.GeneratedStoredProcedure.Schema.SchemaName))
                {
                    continue;
                }
                schemas.Add(new CSchema { SchemaName = storeProcedure.GeneratedStoredProcedure.Schema.SchemaName });

            }

            return schemas;

        }
    }
}