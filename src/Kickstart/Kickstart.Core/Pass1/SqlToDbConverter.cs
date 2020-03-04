using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;
using Kickstart.Pass1.SqlServer;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject.Table;
using Kickstart.Pass2.SqlServer;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass1
{
    public class SqlToDbConverter
    {
        private const string CompanyName = "Company";

        public KDataStoreProject BuildSqlMeta(string connectionString, string outputRootPath,
            KDataStoreProject databaseProject)
        {
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

            if (!string.IsNullOrEmpty(databaseProject.SqlTableText))
            {
                var tableNames = CreateSqlTablesInDb(connectionStringBuilder.ConnectionString,
                    databaseProject.SqlTableText);
                var converter = new SqlServerTableToCTableConverter();
                foreach (var tablePair in tableNames)
                {
                    var reader = new SqlServerTableReader();
                    reader.ConnectionString = connectionStringBuilder.ConnectionString;
                    var dataReader = reader.Read(tablePair.Schema.SchemaName, tablePair.TableName);
                    var foreignKeyData = reader.ReadForeignKeys(tablePair.Schema.SchemaName, tablePair.TableName);
                    
                    var table2 = converter.Convert(dataReader, foreignKeyData);
                    table2.TableText = tablePair.TableText;
                    tablePair.GeneratedTable = table2;
                    databaseProject.Table.Add(tablePair);
                }
                converter.FixupForeignKeyTables(databaseProject.Table.Select(kt=>kt.GeneratedTable));
                foreach (var tablePair in tableNames)
                {
                    tablePair.GeneratedTable.DerivedFrom = tablePair.DerivedFrom;
                }

            }

            if (!string.IsNullOrEmpty(databaseProject.MockSqlViewText))
            {
                var mockViewNames = CreateMockSqlViewsInDb(connectionStringBuilder.ConnectionString,
                    databaseProject.MockSqlViewText);


                foreach (var viewPair in mockViewNames)
                {
                    var reader = new SqlServerViewReader();
                    
                    var dataReader = reader.Read(connectionStringBuilder.ConnectionString, viewPair.ViewName, viewPair.ViewText);
                    var converter = new SqlServerViewToCViewConverter();
                    var view2 = converter.Convert(dataReader);

                    if (databaseProject.KickstartCreatedBy)
                        AddCreatedByToView(view2);
                    if (databaseProject.KickstartCreatedDateUtc)
                        AddCreatedDateUtcToView(view2);
                    if (databaseProject.KickstartModifiedBy)
                        AddModifiedByToView(view2);
                    if (databaseProject.KickstartModifiedDateUtc)
                        AddModifiedDateUtcToView(view2);
                    if (databaseProject.KickstartLoadDateUtc)
                        AddLoadDateUtcToView(view2);
                    if (databaseProject.KickstartEffectiveDateUtc)
                        AddEffectiveDateUtcToView(view2);
                    if (databaseProject.KickstartHashDiff)
                        AddHashDiffToView(view2);

                    viewPair.GeneratedView = view2;
                    databaseProject.MockView.Add(viewPair);
                }
            }
            if (!string.IsNullOrEmpty(databaseProject.SqlTableTypeText))
            {
                var tableTypeNames = CreateTableTypesInDb(connectionStringBuilder.ConnectionString,
                    databaseProject.SqlTableTypeText);


                foreach (var tableType in tableTypeNames)
                {
                    var reader = new SqlServerTableTypeReader();
                    reader.ConnectionString = connectionStringBuilder.ConnectionString;
                    var tt = reader.Read(tableType.Schema, tableType.TableTypeName, tableType.TableTypeText);
                    tableType.GeneratedTableType = tt;
                    databaseProject.TableType.Add(tableType);
                }
            }
            if (!string.IsNullOrEmpty(databaseProject.SqlViewText))
            {
                var viewNames =
                    CreateSqlViewsInDb(connectionStringBuilder.ConnectionString, databaseProject.SqlViewText);

                foreach (var viewPair in viewNames)
                {
                    var reader = new SqlServerViewReader();
                    var dataReader = reader.Read(connectionStringBuilder.ConnectionString, viewPair.ViewName, viewPair.ViewText);
                    var converter = new SqlServerViewToCViewConverter();
                    var view2 = converter.Convert(dataReader);
                    view2.ViewText = viewPair.ViewText;
                    viewPair.GeneratedView = view2;
                    databaseProject.View.Add(viewPair);
                }
            }

            if (!string.IsNullOrEmpty(databaseProject.SqlStoredProcedureText))
            {
                var storedProcedureNames = CreateSqlStoredProcsInDb(connectionStringBuilder.ConnectionString,
                    databaseProject.SqlStoredProcedureText);
                
                foreach (var sp in storedProcedureNames)
                {
                    var reader = new SqlServerStoredProcedureReader( );
                    var storedProceure = reader.Read(connectionStringBuilder.ConnectionString, sp.Schema, sp.StoredProcedureName, sp.StoredProcedureText);
                    sp.GeneratedStoredProcedure = storedProceure;
                    sp.GeneratedStoredProcedure.StoredProcedureDescription = sp.StoredProcedureDescription;
                    sp.GeneratedStoredProcedure.ResultSetName = sp.StoredProcedureName + "Dto";//"ResultSet";
                    sp.ResultSetName = sp.StoredProcedureName + "Dto";// "ResultSet";
                    databaseProject.StoredProcedure.Add(sp);
                }
            }

            DropWorkDb(connectionStringBuilder, newDbName);


            databaseProject.ConfigureMetaData();


            databaseProject.AddSeedData();
            ConvertMockViewsToTables(databaseProject);
            var currentSPs = new List<KStoredProcedure>();
            currentSPs.AddRange(databaseProject.StoredProcedure);
            databaseProject.StoredProcedure.Clear();
            if (databaseProject.KickstartCRUDStoredProcedures)
                AddInsertUpdateStoredProcedures(databaseProject);
            databaseProject.StoredProcedure.AddRange(currentSPs);
            if (databaseProject.KickstartCRUDStoredProcedures)
                AddDeleteStoredProcedures(databaseProject);
            databaseProject.ConfigureMetaData2();

            //todo; clean this up
            foreach (var sp in databaseProject.StoredProcedure)
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
            return databaseProject;
        }

        private void ConvertMockViewsToTables(KDataStoreProject mDatabaseProject)
        {
            foreach (var view in mDatabaseProject.MockView)
            {
                var converter2 = new CViewToCTableConverter();

                var table = converter2.Convert(view.GeneratedView);

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
                var converter3 = new CTableToSStoredProcedureDeleteConverter();

                var storedProcedure = converter3.Convert(table.GeneratedTable);

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
                var converter3 = new CTableToSqlServerStoredProcedureAddUpdateConverter();

                var storedProcedure = converter3.Convert(table.GeneratedTable);
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

        private List<KTable> CreateSqlTablesInDb(string connectionString, string sqlText)
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

                        if (createTableStatement == null)
                            continue;
                        var viewSchemaName = createTableStatement.SchemaObjectName.SchemaIdentifier.Value;

                        var createSchemaCommand = new SqlCommand(
                            $@" IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'{viewSchemaName}')
                                        BEGIN
                                            EXEC sp_executesql N'CREATE SCHEMA {viewSchemaName}'
                                        END", sqlConnection);
                        createSchemaCommand.ExecuteNonQuery();
                        //createViewStatement.SelectStatement.QueryExpression.

                        scriptGen.GenerateScript(statement, out var scriptOut);

                        var sqlCommand = new SqlCommand(scriptOut, sqlConnection);
                        sqlCommand.ExecuteNonQuery();
                        tablesNames.Add(new KTable
                        {
                            Schema =
                                new CSchema {SchemaName = createTableStatement.SchemaObjectName.SchemaIdentifier.Value},
                            TableName =
                                createTableStatement.SchemaObjectName.BaseIdentifier.Value,
                            TableText = scriptOut
                        });
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

        private List<KTableType> CreateTableTypesInDb(string connectionString, string sqlText)
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
                        tableTypes.Add(new KTableType
                        {
                            Schema =
                                createTypeTableStatement.Name.SchemaIdentifier.Value,
                            TableTypeName =
                                createTypeTableStatement.Name.BaseIdentifier.Value,
                            TableTypeText = scriptOut
                        });
                    }
                }

                sqlConnection.Close();
            }
            return tableTypes;
        }

        private List<KStoredProcedure> CreateSqlStoredProcsInDb(string connectionString, string sqlText)
        {
            var storedProcedures = new List<KStoredProcedure>();

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
                         
                        scriptGen.GenerateScript(statement, out var scriptOut);

                        var sqlCommand = new SqlCommand(scriptOut, sqlConnection);
                        sqlCommand.ExecuteNonQuery();
                        storedProcedures.Add(new KStoredProcedure
                        {
                            Schema =
                                createProcedureStatement.ProcedureReference.Name.SchemaIdentifier.Value,
                            StoredProcedureName =
                                createProcedureStatement.ProcedureReference.Name.BaseIdentifier.Value,
                            StoredProcedureText = scriptOut,
                            StoredProcedureDescription = batchText
                        });
                    }
                }
                sqlConnection.Close();
            }
            return storedProcedures;
        }
    }
}