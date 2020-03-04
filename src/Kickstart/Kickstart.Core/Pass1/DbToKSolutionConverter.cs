using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.KModel.Data;
using Kickstart.Pass1.Service;
using Kickstart.Pass1.SqlServer;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject.Table;
using Kickstart.Pass2.SqlServer;
using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Kickstart.Pass2.CModel;

namespace Kickstart.Pass1
{
    public class DbToKSolutionConverter : IDbToKSolutionConverter
    {
        private readonly ISqlServerTableReader _sqlServerTableReader;
        private readonly ISqlServerTableToCTableConverter _sqlServerTableToCTableConverter;
        private readonly ISqlServerFunctionReader _sqlServerFunctionReader;
        //private readonly ISqlServerFunctionToCFunctionConverter _sqlServerFunctionToCFunctionConverter;
        private readonly ISqlServerViewReader _sqlServerViewReader;
        private readonly ISqlServerViewToCViewConverter _sqlServerViewToCViewConverter;
        private readonly ISqlServerTableTypeReader _sqlServerTableTypeReader;
        private readonly ISqlServerStoredProcedureReader _sqlServerStoredProcedureReader;
        private readonly ICViewToCTableConverter _cViewToCTableConverter;
        private readonly ICTableToSStoredProcedureDeleteConverter _cTableToSStoredProcedureDeleteConverter;
        private readonly ICTableToSqlServerStoredProcedureAddUpdateConverter _cTableToSqlServerStoredProcedureAddUpdateConverter;

        public DbToKSolutionConverter(ISqlServerTableReader sqlServerTableReader, ISqlServerTableToCTableConverter sqlServerTableToCTableConverter, ISqlServerViewReader sqlServerViewReader,
            ISqlServerViewToCViewConverter sqlServerViewToCViewConverter, ISqlServerStoredProcedureReader sqlServerStoredProcedureReader, ISqlServerTableTypeReader sqlServerTableTypeReader, ICViewToCTableConverter cViewToCTableConverter, ICTableToSStoredProcedureDeleteConverter cTableToSStoredProcedureDeleteConverter, ICTableToSqlServerStoredProcedureAddUpdateConverter cTableToSqlServerStoredProcedureAddUpdateConverter, ISqlServerFunctionReader sqlServerFunctionReader)
        {
            _sqlServerTableReader = sqlServerTableReader;
            _sqlServerTableToCTableConverter = sqlServerTableToCTableConverter;
            _sqlServerViewReader = sqlServerViewReader;
            _sqlServerViewToCViewConverter = sqlServerViewToCViewConverter;
            _sqlServerTableTypeReader = sqlServerTableTypeReader;
            _sqlServerStoredProcedureReader = sqlServerStoredProcedureReader;
            _cViewToCTableConverter = cViewToCTableConverter;
            _cTableToSStoredProcedureDeleteConverter = cTableToSStoredProcedureDeleteConverter;
            _cTableToSqlServerStoredProcedureAddUpdateConverter = cTableToSqlServerStoredProcedureAddUpdateConverter;
            _sqlServerFunctionReader = sqlServerFunctionReader;
        }

        private const string CompanyName = "Company";

        
        public KDataStoreProject BuildSqlMeta(string connectionString, string outputRootPath,
            KDataStoreProject dataStoreProject)
        {
            
            dataStoreProject.ClearSqlMeta(); //clear what was generated during user config, going to regenerate
            var stopWatch = Stopwatch.StartNew();

            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var timeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            var newDbName = $"WorkDb{timeString}";
            connectionStringBuilder.InitialCatalog = "master";
            connectionStringBuilder.IntegratedSecurity = true;
            //connectionStringBuilder.UserID = "fillthisin";
            //connectionStringBuilder.Password = "passwordhere";
            connectionStringBuilder.ConnectTimeout = 60;

            var scriptGen = new Sql120ScriptGenerator();


            using (var sqlConnection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                sqlConnection.Open();
                var createDatabaseStatement =
                    new CreateDatabaseStatement {DatabaseName = new Identifier {Value = newDbName}};

                string createDatabaseScriptOut;
                scriptGen.GenerateScript(createDatabaseStatement, out createDatabaseScriptOut);
                var sqlCommandCreateDatabase = new SqlCommand(createDatabaseScriptOut, sqlConnection);
                sqlCommandCreateDatabase.ExecuteNonQuery();
                sqlConnection.Close();
            }

            connectionStringBuilder.InitialCatalog = newDbName;

            if (!string.IsNullOrEmpty(dataStoreProject.SqlTableText))
            {
                var tableNames = CreateSqlTablesInDb(dataStoreProject, connectionStringBuilder.ConnectionString,
                    dataStoreProject.SqlTableText);

                foreach (var tablePair in tableNames)
                {
                    _sqlServerTableReader.ConnectionString = connectionStringBuilder.ConnectionString;
                    var dataReader = _sqlServerTableReader.Read(tablePair.Schema.SchemaName, tablePair.TableName);
                    var foreignKeyData = _sqlServerTableReader.ReadForeignKeys(tablePair.Schema.SchemaName, tablePair.TableName);
                    var table2 = _sqlServerTableToCTableConverter.Convert(dataReader, foreignKeyData);
                    table2.TableText = tablePair.TableText;
                    table2.DatabaseType = dataStoreProject.DataStoreType;
                    table2.ConvertToSnakeCase = dataStoreProject.ConvertToSnakeCase;
                    
                    tablePair.GeneratedTable = table2;
                    dataStoreProject.Table.Add(tablePair);
                }

                _sqlServerTableToCTableConverter.FixupForeignKeyTables(dataStoreProject.Table.Select(kt=>kt.GeneratedTable));
                foreach (var tablePair in tableNames)
                {
                    tablePair.GeneratedTable.DerivedFrom = tablePair.DerivedFrom;
                }

            }

            if (!string.IsNullOrEmpty(dataStoreProject.MockSqlViewText))
            {
                var mockViewNames = CreateMockSqlViewsInDb(connectionStringBuilder.ConnectionString,
                    dataStoreProject.MockSqlViewText);


                foreach (var viewPair in mockViewNames)
                {
                    var dataReader = _sqlServerViewReader.Read(connectionStringBuilder.ConnectionString, viewPair.ViewName, viewPair.ViewText);
                    var view2 = _sqlServerViewToCViewConverter.Convert(dataReader);

                    if (dataStoreProject.KickstartCreatedBy)
                        AddCreatedByToView(view2);
                    if (dataStoreProject.KickstartCreatedDateUtc)
                        AddCreatedDateUtcToView(view2);
                    if (dataStoreProject.KickstartModifiedBy)
                        AddModifiedByToView(view2);
                    if (dataStoreProject.KickstartModifiedDateUtc)
                        AddModifiedDateUtcToView(view2);
                    if (dataStoreProject.KickstartLoadDateUtc)
                        AddLoadDateUtcToView(view2);
                    if (dataStoreProject.KickstartEffectiveDateUtc)
                        AddEffectiveDateUtcToView(view2);
                    if (dataStoreProject.KickstartHashDiff)
                        AddHashDiffToView(view2);

                    viewPair.GeneratedView = view2;
                    dataStoreProject.MockView.Add(viewPair);
                }
            }
            if (!string.IsNullOrEmpty(dataStoreProject.SqlTableTypeText))
            {
                var tableTypeNames = CreateTableTypesInDb(dataStoreProject, connectionStringBuilder.ConnectionString,
                    dataStoreProject.SqlTableTypeText);


                foreach (var tableType in tableTypeNames)
                {
                    _sqlServerTableTypeReader.ConnectionString = connectionStringBuilder.ConnectionString;
                    var tt = _sqlServerTableTypeReader.Read(tableType.Schema, tableType.TableTypeName, tableType.TableTypeText);
                    tt.DerivedFrom = tableType.DerivedFrom as CPart;
                    tt.DatabaseType = dataStoreProject.DataStoreType;
                    tt.ConvertToSnakeCase = dataStoreProject.ConvertToSnakeCase;

                    tableType.GeneratedTableType = tt;
                    
                    dataStoreProject.TableType.Add(tableType);
                }
            }
            if (!string.IsNullOrEmpty(dataStoreProject.SqlViewText))
            {
                var viewNames =
                    CreateSqlViewsInDb(connectionStringBuilder.ConnectionString, dataStoreProject.SqlViewText);

                foreach (var viewPair in viewNames)
                {
                    
                    var dataReader = _sqlServerViewReader.Read(connectionStringBuilder.ConnectionString, viewPair.ViewName, viewPair.ViewText);
                    
                    var view2 = _sqlServerViewToCViewConverter.Convert(dataReader);
                    view2.DatabaseType = dataStoreProject.DataStoreType;
                    view2.ConvertToSnakeCase = dataStoreProject.ConvertToSnakeCase;

                    view2.ViewText = viewPair.ViewText;
                    viewPair.GeneratedView = view2;
                    dataStoreProject.View.Add(viewPair);
                }
            }
            if (!string.IsNullOrEmpty(dataStoreProject.SqlFunctionText))
            {
                var functionNames =
                    CreateSqlFunctionsInDb(dataStoreProject, connectionStringBuilder.ConnectionString, dataStoreProject.SqlFunctionText);

                foreach (var functionPair in functionNames)
                {

                    var function2 = _sqlServerFunctionReader.Read(connectionStringBuilder.ConnectionString, functionPair.Schema.SchemaName,  functionPair.FunctionName, functionPair.FunctionText);

                    //var function2 = _sqlServerFunctionToCFunctionConverter.Convert(dataReader);
                    functionPair.DatabaseType = dataStoreProject.DataStoreType;
                    functionPair.ConvertToSnakeCase = dataStoreProject.ConvertToSnakeCase;

                    functionPair.FunctionText = functionPair.FunctionText;
                    functionPair.GeneratedFunction = functionPair;
                    dataStoreProject.Function.Add(functionPair);
                }
            }

            if (!string.IsNullOrEmpty(dataStoreProject.SqlStoredProcedureText))
            {
                var storedProcedureNames = CreateSqlStoredProcsInDb(dataStoreProject, connectionStringBuilder.ConnectionString,
                    dataStoreProject.SqlStoredProcedureText);


                foreach (var sp in storedProcedureNames)
                {
                   
                    var storedProceure = _sqlServerStoredProcedureReader.Read(connectionStringBuilder.ConnectionString, sp.Schema, sp.StoredProcedureName, sp.StoredProcedureText);
                    storedProceure.DatabaseType = dataStoreProject.DataStoreType;
                    storedProceure.DerivedFrom = sp.DerivedFrom as CPart;
                    storedProceure.ConvertToSnakeCase = dataStoreProject.ConvertToSnakeCase;

                    sp.GeneratedStoredProcedure = storedProceure;
                    sp.GeneratedStoredProcedure.StoredProcedureDescription = sp.StoredProcedureDescription;
                    sp.GeneratedStoredProcedure.GenerateAsEmbeddedQuery = dataStoreProject.GenerateStoredProcAsEmbeddedQuery;
                    dataStoreProject.StoredProcedure.Add(sp);
                }
            }

            if (dataStoreProject.Query.Any())
            {
                foreach (var kQuery in dataStoreProject.Query)
                {
                    
                        var reader =
                            new SqlServerQueryReader
                            {
                                ConnectionString = connectionStringBuilder.ConnectionString
                            };

                    kQuery.GeneratedQuery = new CQuery();
                    //databaseProject.Query.Add()
                        /*
                        var query = reader.Read(sp.Schema, sp.StoredProcedureName, sp.StoredProcedureText);
                        sp.GeneratedStoredProcedure = storedProceure;
                        sp.GeneratedStoredProcedure.StoredProcedureDescription = sp.StoredProcedureDescription;
                        databaseProject.StoredProcedure.Add(sp);
                        */
                }
            }

            DropWorkDb(connectionStringBuilder, newDbName);


            dataStoreProject.ConfigureMetaData();


            dataStoreProject.AddSeedData();
            ConvertMockViewsToTables(dataStoreProject);
            var currentSPs = new List<KStoredProcedure>();
            currentSPs.AddRange(dataStoreProject.StoredProcedure);
            dataStoreProject.StoredProcedure.Clear();
            if (dataStoreProject.KickstartCRUDStoredProcedures)
                AddInsertUpdateStoredProcedures(dataStoreProject);
            dataStoreProject.StoredProcedure.AddRange(currentSPs);
            if (dataStoreProject.KickstartCRUDStoredProcedures)
                AddDeleteStoredProcedures(dataStoreProject);
            dataStoreProject.ConfigureMetaData2();

            //todo; clean this up
            foreach (var sp in dataStoreProject.StoredProcedure)
            {
                sp.GeneratedStoredProcedure.ParameterSetName = sp.ParameterSetName;
                sp.GeneratedStoredProcedure.ResultSetName = sp.ResultSetName;
                sp.GeneratedStoredProcedure.ReturnsMultipleRows = sp.ReturnsMultipleRows;
            }
            /*
            var protoRpcRefs = new List<SProtoRpcRef>();
            foreach (var protoRpcRef in mSolution.ProtoRpcRef)
            {

                protoRpcRefs.Add(protoRpcRef);
            }*/
            return dataStoreProject;
        }

        

        private void ExecuteQueriesOnDb(string connectionString, string sqlStoredProcedureText)
        {
            throw new NotImplementedException();
        }

        private void ConvertMockViewsToTables(KDataStoreProject mDatabaseProject)
        {
            foreach (var view in mDatabaseProject.MockView)
            {
                var table = _cViewToCTableConverter.Convert(view.GeneratedView);

                mDatabaseProject.Table.Add(new KTable
                {
                    Schema = new CSchema {SchemaName = table.Schema.SchemaName},
                    TableName = table.TableName,
                    GeneratedTable = table
                });
            }
            mDatabaseProject.MockView.Clear();
        }


        private void AddDeleteStoredProcedures(KDataStoreProject mDatabaseProject)
        {
            var deleteStoredProcedures = new List<KStoredProcedure>();
            foreach (var table in Enumerable.Reverse(mDatabaseProject.Table))
            {
                var storedProcedure = _cTableToSStoredProcedureDeleteConverter.Convert(table.GeneratedTable);

                deleteStoredProcedures.Add(new KStoredProcedure
                {
                    StoredProcedureName = storedProcedure.StoredProcedureName,
                    ParameterSetName = storedProcedure.ParameterSetName,
                    ResultSetName = storedProcedure.ResultSetName,
                    GeneratedStoredProcedure = storedProcedure
                });
            }

            mDatabaseProject.StoredProcedure.AddRange(deleteStoredProcedures);
        }

        private void AddInsertUpdateStoredProcedures(KDataStoreProject mDatabaseProject)
        {
            //merge version of stored procs
            //update stored procedures
            var addUpdateStoredProcedures = new List<KStoredProcedure>();
            foreach (var table in mDatabaseProject.Table)
            {
                var storedProcedure = _cTableToSqlServerStoredProcedureAddUpdateConverter.Convert(table.GeneratedTable);
                addUpdateStoredProcedures.Add(new KStoredProcedure
                {
                    StoredProcedureName = storedProcedure.StoredProcedureName,
                    ParameterSetName = storedProcedure.ParameterSetName,
                    ResultSetName = storedProcedure.ResultSetName,
                    GeneratedStoredProcedure = storedProcedure
                });
            }


            mDatabaseProject.StoredProcedure.AddRange(addUpdateStoredProcedures);
        }

        private void AddCreatedByToView(CView view2)
        {
            if (view2.Column.Exists(c => c.ColumnName.ToLower() == "createdby"))
                return;
            view2.Column.Add(new CColumn(view2)
            {
                ColumnName = "CreatedBy",
                ColumnType = DbType.AnsiString,
                ColumnTypeRaw = "varchar",
                ColumnLength = 255,
                IsNullable = false
            });
        }

        private void AddModifiedByToView(CView view2)
        {
            if (view2.Column.Exists(c => c.ColumnName.ToLower() == "modifiedby"))
                return;
            view2.Column.Add(new CColumn(view2)
            {
                ColumnName = "ModifiedBy",
                ColumnType = DbType.AnsiString,
                ColumnTypeRaw = "varchar",
                ColumnLength = 255,
                IsNullable = true
            });
        }

        private void AddCreatedDateUtcToView(CView view2)
        {
            if (view2.Column.Exists(c => c.ColumnName.ToLower() == "createddateutc"))
                return;
            view2.Column.Add(new CColumn(view2)
            {
                ColumnName = "CreatedDateUtc",
                ColumnType = DbType.DateTime2,
                ColumnTypeRaw = "datetime2",
                IsNullable = false
            });
        }

        private void AddModifiedDateUtcToView(CView view2)
        {
            if (view2.Column.Exists(c => c.ColumnName.ToLower() == "modifieddateutc"))
                return;
            view2.Column.Add(new CColumn(view2)
            {
                ColumnName = "ModifiedDateUtc",
                ColumnType = DbType.DateTime2,
                ColumnTypeRaw = "datetime2",
                IsNullable = true
            });
        }

        private void AddEffectiveDateUtcToView(CView view2)
        {
            if (view2.Column.Exists(c => c.ColumnName.ToLower() == "effectivedateutc"))
                return;
            view2.Column.Add(new CColumn(view2)
            {
                ColumnName = "EffectiveDateUtc",
                ColumnType = DbType.DateTime2,
                ColumnTypeRaw = "datetime2",
                IsNullable = false
            });
        }

        private void AddLoadDateUtcToView(CView view2)
        {
            if (view2.Column.Exists(c => c.ColumnName.ToLower() == "loaddateutc"))
                return;
            view2.Column.Add(new CColumn(view2)
            {
                ColumnName = "LoadDateUTC",
                ColumnType = DbType.DateTime2,
                ColumnTypeRaw = "datetime2",
                IsNullable = false
            });
        }

        private void AddHashDiffToView(CView view2)
        {
            if (view2.Column.Exists(c => c.ColumnName.ToLower() == "hashdiff"))
                return;
            view2.Column.Add(new CColumn(view2)
            {
                ColumnName = "HashDiff",
                ColumnType = DbType.AnsiString,
                ColumnSqlDbType = SqlDbType.VarChar,
                ColumnLength = 32,
                ColumnTypeRaw = "char",
                IsNullable = false
            });
        }

        private void DropWorkDb(SqlConnectionStringBuilder connectionStringBuilder, string newDbName)
        {
            connectionStringBuilder.InitialCatalog = "master";
            using (var sqlConnection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                sqlConnection.Open();
                var dropDatabaseStatement = new DropDatabaseStatement();
                dropDatabaseStatement.Databases.Add(new Identifier {Value = newDbName});


                var scriptGen = new Sql120ScriptGenerator();


                scriptGen.GenerateScript(dropDatabaseStatement, out var dropDatabaseScriptOut);
                var sqlCommandEnableDrop =
                    new SqlCommand($"ALTER DATABASE [{newDbName}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE",
                        sqlConnection);
                sqlCommandEnableDrop.ExecuteNonQuery();
                var sqlCommanDropDatabase = new SqlCommand(dropDatabaseScriptOut, sqlConnection);
                sqlCommanDropDatabase.ExecuteNonQuery();

                sqlConnection.Close();
            }
        }

        private List<KTable> CreateSqlTablesInDb(KDataStoreProject kDataStoreProject, string connectionString, string sqlText)
        {
            var tablesNames = new List<KTable>();
            var scriptGen = new Sql120ScriptGenerator();

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var parser = new TSql120Parser(false);

                 
                var script2 = parser.Parse(new StringReader(sqlText), out var errors) as TSqlScript;
                if (errors.Count > 0)
                {
                    var errorList = new StringBuilder();
                    foreach (var error in errors)
                        errorList.AppendLine($"{error.Message}<br/>");
                    throw new ApplicationException(errorList.ToString());
                }
                foreach (var batch2 in script2.Batches)
                {
                    foreach (var statement in batch2.Statements)
                    {
                        var createTableStatement = statement as CreateTableStatement;
                        var alterTableStatement = statement as AlterTableStatement;

                        if (createTableStatement == null && alterTableStatement == null)
                            continue;

                        string viewSchemaName = string.Empty;

                        if (createTableStatement != null)
                        {
                             viewSchemaName = createTableStatement.SchemaObjectName.SchemaIdentifier.Value;
                        }
                        else if (alterTableStatement != null)
                        {
                            viewSchemaName = alterTableStatement.SchemaObjectName.SchemaIdentifier.Value;

                        }

                        var createSchemaCommand = new SqlCommand(
                                $@" IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'{viewSchemaName}')
                                        BEGIN
                                            EXEC sp_executesql N'CREATE SCHEMA [{viewSchemaName}]'
                                        END", sqlConnection);
                            createSchemaCommand.ExecuteNonQuery();
                        
                        scriptGen.GenerateScript(statement, out var scriptOut);

                        var sqlCommand = new SqlCommand(scriptOut, sqlConnection);
                        sqlCommand.ExecuteNonQuery();

                        if (createTableStatement != null)
                        {
                            var oldKTable = kDataStoreProject.OldTable.FirstOrDefault(t =>
                                t.Schema.SchemaName == createTableStatement.SchemaObjectName.SchemaIdentifier.Value &&
                                t.TableName == createTableStatement.SchemaObjectName.BaseIdentifier.Value);

                            //copy some attributes

                            var newKTable = new KTable
                            {
                                Schema =
                                    new CSchema
                                    {
                                        SchemaName = createTableStatement.SchemaObjectName.SchemaIdentifier.Value
                                    },
                                TableName =
                                    createTableStatement.SchemaObjectName.BaseIdentifier.Value,
                                TableText = scriptOut
                            };

                            if (oldKTable != null)
                            {
                                newKTable.DerivedFrom = oldKTable.DerivedFrom;
                            }

                            tablesNames.Add(newKTable);
                        }
                        else if (alterTableStatement != null)
                        {
                            //assumes ALTER TABLE come before CREATE TABLE
                            var existingTable = tablesNames.Single(t =>
                                t.Schema.SchemaName == alterTableStatement.SchemaObjectName.SchemaIdentifier.Value &&
                                t.TableName == alterTableStatement.SchemaObjectName.BaseIdentifier.Value);

                            existingTable.TableText +=  Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine ;
                            existingTable.TableText += scriptOut;
                            existingTable.TableText += Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine;

                        }
                    }
                }

                sqlConnection.Close();
            }
            return tablesNames;
        }

        private List<KView> CreateSqlViewsInDb(string connectionString, string sqlText)
        {
            var viewNames = new List<KView>();
            var scriptGen = new Sql120ScriptGenerator();

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var parser = new TSql120Parser(false);

                
                var script2 = parser.Parse(new StringReader(sqlText), out var errors) as TSqlScript;
                if (errors.Count > 0)
                {
                    var errorList = new StringBuilder();
                    foreach (var error in errors)
                        errorList.AppendLine($"{error.Message}<br/>");
                    throw new ApplicationException(errorList.ToString());
                }
                foreach (var batch2 in script2.Batches)
                {
                    foreach (var statement in batch2.Statements)
                    {
                        var createViewStatement = statement as CreateViewStatement;

                        if (createViewStatement == null)
                            continue;
                        var viewSchemaName = createViewStatement.SchemaObjectName.SchemaIdentifier.Value;

                        var createSchemaCommand = new SqlCommand(
                            $@" IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'{viewSchemaName}')
                                        BEGIN
                                            EXEC sp_executesql N'CREATE SCHEMA {viewSchemaName}'
                                        END", sqlConnection);
                        createSchemaCommand.ExecuteNonQuery();
                        //createViewStatement.SelectStatement.QueryExpression.
                        string scriptOut;

                        scriptGen.GenerateScript(statement, out scriptOut);

                        var sqlCommand = new SqlCommand(scriptOut, sqlConnection);
                        sqlCommand.ExecuteNonQuery();
                        viewNames.Add(new KView
                        {
                            Schema =
                                new CSchema {SchemaName = createViewStatement.SchemaObjectName.SchemaIdentifier.Value},
                            ViewName =
                                createViewStatement.SchemaObjectName.BaseIdentifier.Value,
                            ViewText = scriptOut
                        });
                    }
                }

                sqlConnection.Close();
            }
            return viewNames;
        }

        private List<KView> CreateMockSqlViewsInDb(string connectionString, string sqlText)
        {
            var viewNames = new List<KView>();
            var scriptGen = new Sql120ScriptGenerator();

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var parser = new TSql120Parser(false);

                 
                var script2 = parser.Parse(new StringReader(sqlText), out var errors) as TSqlScript;
                if (errors.Count > 0)
                {
                    var errorList = new StringBuilder();
                    foreach (var error in errors)
                        errorList.AppendLine($"{error.Message}<br/>");
                    throw new ApplicationException(errorList.ToString());
                }
                for (var batchNo = 0; batchNo < script2.Batches.Count; batchNo++)
                {
                    var batch2 = script2.Batches[batchNo];
                    foreach (var statement in batch2.Statements)
                    {
                        var createViewStatement = statement as CreateViewStatement;

                        if (createViewStatement == null)
                            continue;
                        var viewSchemaName = createViewStatement.SchemaObjectName.SchemaIdentifier.Value;

                        var createSchemaCommand = new SqlCommand(
                            $@" IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'{viewSchemaName}')
                                    BEGIN
                                        EXEC sp_executesql N'CREATE SCHEMA {viewSchemaName}'
                                    END", sqlConnection);
                        createSchemaCommand.ExecuteNonQuery();
                        //createViewStatement.SelectStatement.QueryExpression.
                        string scriptOut;

                        scriptGen.GenerateScript(statement, out scriptOut);
                        //var batchStartLineNo = batch2.StartLine;
                        var batchStartCharNo = batch2.StartOffset;
                        var batchCharLength = batch2.FragmentLength;
                        /*
                        if (batchNo < script2.Batches.Count)
                            batchEndCharNo = script2.Batches[batchNo].StartOffset - 1;
                        else
                            batchEndCharNo = sqlText.Length;
                            */
                        var batchScript = sqlText.Substring(batchStartCharNo, batchCharLength);

                        var sqlCommand = new SqlCommand(scriptOut, sqlConnection);
                        sqlCommand.ExecuteNonQuery();

                        viewNames.Add(new KView
                        {
                            Schema =
                                new CSchema {SchemaName = createViewStatement.SchemaObjectName.SchemaIdentifier.Value},
                            ViewName =
                                createViewStatement.SchemaObjectName.BaseIdentifier.Value,
                            ViewText = batchScript
                        });
                    }
                }
                sqlConnection.Close();
            }
            return viewNames;
        }

        private List<KTableType> CreateTableTypesInDb(KDataStoreProject kDataStoreProject, string connectionString, string sqlText)
        {
            var tableTypes = new List<KTableType>();

            var scriptGen = new Sql120ScriptGenerator();

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var parser = new TSql120Parser(false);

                 
                var script2 = parser.Parse(new StringReader(sqlText), out var errors) as TSqlScript;
                if (errors.Count > 0)
                {
                    var errorList = new StringBuilder();
                    foreach (var error in errors)
                        errorList.AppendLine($"{error.Message}<br/>");
                    throw new ApplicationException(errorList.ToString());
                }
                foreach (var batch2 in script2.Batches)
                {
                    foreach (var statement in batch2.Statements)
                    {
                        var createTypeTableStatement = statement as CreateTypeTableStatement;

                        if (createTypeTableStatement == null)
                            continue;
                        var viewSchemaName = createTypeTableStatement.Name.SchemaIdentifier.Value;

                        var createSchemaCommand = new SqlCommand(
                            $@" IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'{viewSchemaName}')
                                        BEGIN
                                            EXEC sp_executesql N'CREATE SCHEMA {viewSchemaName}'
                                        END", sqlConnection);
                        createSchemaCommand.ExecuteNonQuery();

                        scriptGen.GenerateScript(statement, out var scriptOut);

                        var sqlCommand = new SqlCommand(scriptOut, sqlConnection);
                        sqlCommand.ExecuteNonQuery();

                        var tableType = new KTableType
                        {
                            Schema =
                                createTypeTableStatement.Name.SchemaIdentifier.Value,
                            TableTypeName =
                                createTypeTableStatement.Name.BaseIdentifier.Value,
                            TableTypeText = scriptOut
                        };

                        
                        var oldTableType = kDataStoreProject.OldTableType.FirstOrDefault(tt =>tt.TableTypeName == tableType.TableTypeName); //todo: compare schema
                        if (oldTableType != null)
                        {
                            tableType.DerivedFrom = oldTableType.DerivedFrom;
                        }

                        tableTypes.Add(tableType);

                    }
                }

                sqlConnection.Close();
            }
            return tableTypes;
        }

        private List<KStoredProcedure> CreateSqlStoredProcsInDb(KDataStoreProject kDataStoreProject, string connectionString, string sqlText)
        {
            var storedProcedures = new List<KStoredProcedure>();

            var scriptGen = new Sql120ScriptGenerator( new SqlScriptGeneratorOptions { IncludeSemicolons = true });
            
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var parser = new TSql120Parser(false);
                 
                var script2 = parser.Parse(new StringReader(sqlText), out var errors) as TSqlScript;
                if (errors.Count > 0)
                {
                    var errorList = new StringBuilder();
                    foreach (var error in errors)
                        errorList.AppendLine($"{error.Message}<br/>");
                    throw new ApplicationException(errorList.ToString());
                }
                for (var batchNo = 0; batchNo < script2.Batches.Count; batchNo++)
                {
                    var batch2 = script2.Batches[batchNo];
                    //get the doc above the stored proc
                    var betweenBatchStart = 0;
                    if (batchNo > 0)
                        betweenBatchStart = script2.Batches[batchNo - 1].StartOffset +
                                            script2.Batches[batchNo - 1].FragmentLength;

                    var betweenBatchEnd = batch2.StartOffset - 1;
                    string batchText = null;
                    if (betweenBatchEnd > 0)
                    {
                        batchText = sqlText.Substring(betweenBatchStart, betweenBatchEnd - betweenBatchStart);
                        //clean up the doc
                        batchText = batchText.Replace("GO", "");
                        batchText = batchText.Replace(Environment.NewLine, "");
                        batchText = batchText.Replace("\r", "");
                        batchText = batchText.Replace("/*", "");
                        batchText = batchText.Replace("*/", "");
                        batchText = batchText.Trim();
                    }
                    foreach (var statement in batch2.Statements)
                    {
                        var createProcedureStatement = statement as CreateProcedureStatement;
                        
                        if (createProcedureStatement == null)
                            continue;
                        var viewSchemaName = createProcedureStatement.ProcedureReference.Name.SchemaIdentifier.Value;

                        var createSchemaCommand = new SqlCommand(
                            $@" IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'{viewSchemaName}')
                                    BEGIN
                                        EXEC sp_executesql N'CREATE SCHEMA {viewSchemaName}'
                                    END", sqlConnection);
                        createSchemaCommand.ExecuteNonQuery();

                        scriptGen.GenerateScript(statement, out var scriptOut, out var errors2);

                        //fixup CTE
                        //var tempScript = scriptOut.Replace(Environment.NewLine, " ").ToUpper();
                        //might insert extra ; it won't hurt anything
                        if (scriptOut.Contains(" WITH "))
                        {
                            scriptOut = scriptOut.Replace(" WITH ", "; WITH ");
                        }
                        else if (scriptOut.Contains(Environment.NewLine+"WITH "))
                        {
                            scriptOut = scriptOut.Replace(Environment.NewLine + "WITH ", ";" + Environment.NewLine + " WITH ");
                        }
                        var sqlCommand = new SqlCommand(scriptOut, sqlConnection);
                        sqlCommand.ExecuteNonQuery();

                        var storedProcedure = new KStoredProcedure
                        {
                            Schema =
                                createProcedureStatement.ProcedureReference.Name.SchemaIdentifier.Value,
                            StoredProcedureName =
                                createProcedureStatement.ProcedureReference.Name.BaseIdentifier.Value,
                            StoredProcedureText = scriptOut,
                            StoredProcedureDescription = batchText,
                            ResultSetName = BuildResultSetName(createProcedureStatement.ProcedureReference.Name.BaseIdentifier.Value)  
                        };


                        var oldStoredProc = kDataStoreProject.OldStoredProcedure.FirstOrDefault(s => s.StoredProcedureName == storedProcedure.StoredProcedureName); //todo: compare schema
                        if (oldStoredProc != null)
                        {
                            storedProcedure.DerivedFrom = oldStoredProc.DerivedFrom; 
                        }
                        storedProcedures.Add(storedProcedure);
                    }
                }
                sqlConnection.Close();
            }
            return storedProcedures;
        }

        private string BuildResultSetName(string baseIdentifierValue)
        {
            var name = ActionPrefixRemover.Remove(baseIdentifierValue); 

            var s = new Inflector.Inflector(CultureInfo.CurrentCulture);

            name = s.Singularize(name);

            return name + "Dto"; ;
        }

        private List<KFunction> CreateSqlFunctionsInDb(KDataStoreProject kDataStoreProject, string connectionString, string sqlText)
        {
            var functions = new List<KFunction>();

            var scriptGen = new Sql120ScriptGenerator(new SqlScriptGeneratorOptions { IncludeSemicolons = true });

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var parser = new TSql120Parser(false);

                var script2 = parser.Parse(new StringReader(sqlText), out var errors) as TSqlScript;
                if (errors.Count > 0)
                {
                    var errorList = new StringBuilder();
                    foreach (var error in errors)
                        errorList.AppendLine($"{error.Message}<br/>");
                    throw new ApplicationException(errorList.ToString());
                }
                for (var batchNo = 0; batchNo < script2.Batches.Count; batchNo++)
                {
                    var batch2 = script2.Batches[batchNo];
                    //get the doc above the stored proc
                    var betweenBatchStart = 0;
                    if (batchNo > 0)
                        betweenBatchStart = script2.Batches[batchNo - 1].StartOffset +
                                            script2.Batches[batchNo - 1].FragmentLength;

                    var betweenBatchEnd = batch2.StartOffset - 1;
                    string batchText = null;
                    if (betweenBatchEnd > 0)
                    {
                        batchText = sqlText.Substring(betweenBatchStart, betweenBatchEnd - betweenBatchStart);
                        //clean up the doc
                        batchText = batchText.Replace("GO", "");
                        batchText = batchText.Replace(Environment.NewLine, "");
                        batchText = batchText.Replace("\r", "");
                        batchText = batchText.Replace("/*", "");
                        batchText = batchText.Replace("*/", "");
                        batchText = batchText.Trim();
                    }
                    foreach (var statement in batch2.Statements)
                    {
                        var createFunctionStatement = statement as CreateFunctionStatement;

                        if (createFunctionStatement == null)
                            continue;
                        var viewSchemaName = createFunctionStatement.Name.SchemaIdentifier.Value;

                        var createSchemaCommand = new SqlCommand(
                            $@" IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'{viewSchemaName}')
                                    BEGIN
                                        EXEC sp_executesql N'CREATE SCHEMA {viewSchemaName}'
                                    END", sqlConnection);
                        createSchemaCommand.ExecuteNonQuery();

                        scriptGen.GenerateScript(statement, out var scriptOut, out var errors2);

                        //fixup CTE
                        //var tempScript = scriptOut.Replace(Environment.NewLine, " ").ToUpper();
                        //might insert extra ; it won't hurt anything
                        if (scriptOut.Contains(" WITH "))
                        {
                            scriptOut = scriptOut.Replace(" WITH ", "; WITH ");
                        }
                        else if (scriptOut.Contains(Environment.NewLine + "WITH "))
                        {
                            scriptOut = scriptOut.Replace(Environment.NewLine + "WITH ", ";" + Environment.NewLine + " WITH ");
                        }
                        var sqlCommand = new SqlCommand(scriptOut, sqlConnection);
                        sqlCommand.ExecuteNonQuery();

                        var function = new KFunction (DataStoreTypes.SqlServer)
                        {
                            Schema = new CSchema() {  SchemaName = 
                                createFunctionStatement.Name.SchemaIdentifier.Value},
                            FunctionName =
                                createFunctionStatement.Name.BaseIdentifier.Value,
                            FunctionText = scriptOut,
                            //StoredProcedureDescription = batchText,
                            ResultSetName = createFunctionStatement.Name.BaseIdentifier.Value + "Dto"// "ResultSet"
                        };

                        /*
                        var oldStoredProc = kDataStoreProject.OldStoredProcedure.FirstOrDefault(s => s.StoredProcedureName == function.FunctionName); //todo: compare schema
                        if (oldStoredProc != null)
                        {
                            function.DerivedFrom = oldStoredProc.DerivedFrom;
                        }
                        */
                        functions.Add(function);
                    }
                }
                sqlConnection.Close();
            }
            return functions;
        }
    }
}