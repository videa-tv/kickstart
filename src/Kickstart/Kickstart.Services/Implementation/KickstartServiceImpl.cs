using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Grpc.Core;
using System.Linq;
using Kickstart.Services.Types;
using Kickstart.Services.Query;
using static Kickstart.Services.Types.KickstartServiceApi;
using Kickstart.Pass1;
using Kickstart.Pass1.KModel;
using Kickstart.Utility;
using Kickstart.Pass2.SqlServer;
using System.IO;
using System.IO.Compression;
using Kickstart.Pass2.DataStoreProject;
using Kickstart.Pass2.CModel.AWS.DMS;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass0.Model;
using Kickstart.Pass1.Service;
using Kickstart.Pass1.SqlServer;
using Microsoft.Extensions.Configuration;
using Kickstart.Services.NetCore.GrpcCommon.Infrastructure;

namespace Kickstart.Services
{
    public class KickstartServiceImpl : KickstartServiceApiBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMediatorExecutor _executor;
        private readonly IFlywayFileNameService _flywayFileNameService;
        private readonly IDbToKSolutionConverter _dbToKSolutionConverter;
        private readonly ICTableToDmsRuleConverter _cTableToDmsRuleConverter;
        private readonly ISqlServerTablesReader _sqlServerTablesReader;
        public KickstartServiceImpl(IConfiguration configuration, IMediatorExecutor executor, IFlywayFileNameService flywayFileNameService, IDbToKSolutionConverter dbToKSolutionConverter, ICTableToDmsRuleConverter cTableToDmsRuleConverter, ISqlServerTablesReader sqlServerTablesReader)
        {
            _configuration = configuration;
            _executor = executor;
            _flywayFileNameService = flywayFileNameService;
            _dbToKSolutionConverter = dbToKSolutionConverter;
            _cTableToDmsRuleConverter = cTableToDmsRuleConverter;
            _sqlServerTablesReader = sqlServerTablesReader;
        }
        public override async Task<KickstartSolutionResponse> KickstartSolution(KickstartSolutionRequest request, ServerCallContext context)
        {
            var workDir = GetTemporaryDirectory();
            var model = new KickstartWizardModel
            {
                ProjectDirectory = workDir,
                MetadataSource = MetadataSource.Grpc,
                ConvertToSnakeCase = request.ConvertToSnakeCase,
                CreateDataLayerProject = request.GenerateDataLayerProject,
                CreateDatabaseProject = request.GenerateDatabaseProject,
                CreateGrpcServiceProject = request.GenerateGrpcServiceProject,
                CreateGrpcUnitTestProject = request.GenerateGrpcUnitTestProject,
                CreateGrpcClientProject = request.GenerateGrpcClientProject,
                CreateWebAppProject = request.GenerateWebAppProject,
                CreateDockerComposeProject = request.GenerateDockerComposeProject,
                CompanyName = request.CompanyName,
                SolutionName = request.SolutionName,
                ProjectName = request.ProjectName,
                DatabaseType = (Utility.DataStoreTypes) request.DatabaseType,
                ProtoFileText = request.ProtoFileText,
                GenerateStoredProcAsEmbeddedQuery = true
            };
            try
            {
                var result = await _executor.ExecuteAsync(new BuildSolutionQuery
                {
                    KickstartModel = model
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var response = new KickstartSolutionResponse
                {
                   Succeeded = false,
                   ErrorMessage = ex.Message
                };
                return response;
            }
            var tempZip = Path.GetTempFileName();
            File.Delete(tempZip);
            System.IO.Compression.ZipFile.CreateFromDirectory(workDir, tempZip);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (FileStream file = new FileStream(tempZip, FileMode.Open, FileAccess.Read))
                    file.CopyTo(memoryStream);

                File.Delete(tempZip);
                ClearFolder(workDir);
                Directory.Delete(workDir);
                var response = new KickstartSolutionResponse
                {
                    Succeeded = true,
                    GeneratedFilesBase64 = Convert.ToBase64String(memoryStream.ToArray())
                };

                return response;
                
                
            }
            
            
        }
        private void ClearFolder(string folderName)
        {
            DirectoryInfo dir = new DirectoryInfo(folderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                di.Delete();
            }
        }
        string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
        public override async Task<ConvertDDLResponse> ConvertDDL(ConvertDDLRequest request, ServerCallContext context)
        {
            var sw = Stopwatch.StartNew();
            GrpcEnvironment.Logger.Info($@"Processing ConvertDDL Request");

            var databaseProject = new KDataStoreProject() {
                KickstartCRUDStoredProcedures = false, ConvertToSnakeCase = request.ConvertToSnakeCase,
                DataStoreType = Utility.DataStoreTypes.Postgres,
                SqlTableText = request.UnconvertedTableDDL,
                SqlTableTypeText = request.UnconvertedTableTypeDDL,
                SqlStoredProcedureText = request.UnconvertedStoredProcedureDDL
            };

            var connectionString = "Server=localhost;";
            var outputRootPath = string.Empty;

            _dbToKSolutionConverter.BuildSqlMeta(connectionString, outputRootPath, databaseProject);

            var response = new ConvertDDLResponse();
            ICSchemaToSchemaConverter schemaConverter;
            ICTableToTableConverter tableConverter;
            ICTableTypeToTableTypeConverter tableTypeConverter;
            ICStoredProcedureToStoredProcedureConverter storedProcConverter;

            if (request.DatabaseType == Types.DatabaseTypes.Postgres)
            {
                schemaConverter = new CSchemaToPostgresSchemaConverter();
                tableConverter = new CTableToPostgresTableConverter();
                tableTypeConverter = new CTableTypeToPostgresTableTypeConverter();
                storedProcConverter = new CStoredProcedureToPostgresStoredProcedureConverter();

            }
            else if (request.DatabaseType == Types.DatabaseTypes.SqlServer)
            {
                schemaConverter = new CSchemaToSqlServerSchemaConverter();
                tableConverter = new CTableToSqlServerTableConverter();
                tableTypeConverter = new CTableTypeToSqlServerTableTypeConverter();
                storedProcConverter = new CStoredProcedureToSSqlServerStoredProcedureConverter();

            }
            else
                throw new NotImplementedException();

            var codeWriter = new CodeWriter();
            var codeWriterTableType = new CodeWriter();

            var codeWriterStoredProc = new CodeWriter();
            
            using (var memoryStream = new MemoryStream( ))
            {
                using (var zipFile = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                {
                    foreach (var schema in databaseProject.GetAllGeneratedSchemas())
                    {
                        if (request.DatabaseType == Types.DatabaseTypes.SqlServer && schema.SchemaName.ToLower() == "dbo")
                        {
                            continue;// dbo already exists on Sql Server
                        }

                        var ddl = schemaConverter.Convert(schema);

                        var fileName = _flywayFileNameService.GetFlywayFileName(schema);
                        //fileWriter.WriteFile(fileName, ddl);
                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations", fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(ddl);
                        }
                    }
                    foreach (var table in databaseProject.Table)
                    {
                        var ddl = tableConverter.Convert(table.GeneratedTable);
                        codeWriter.WriteLine(ddl);
                        codeWriter.WriteLine();

                        var fileName = _flywayFileNameService.GetFlywayFileName(table.GeneratedTable);
                        //fileWriter.WriteFile(fileName, ddl);
                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations",fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(ddl);
                        }
                    }
                    {
                        var rules = new CRules();

                        foreach (var table in databaseProject.Table)
                        {
                            var rList = _cTableToDmsRuleConverter.Convert(table.GeneratedTable);
                            rules.Rule.AddRange(rList);
                        }

                        var json = rules.ToJson();
                        var fileName = "DmsRules.json";
                        var newEntry = zipFile.CreateEntry(fileName);
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(json);
                        }
                        response.ConvertedDmsJson = json;
                    }
                    

                    foreach (var tableType in databaseProject.TableType)
                    {
                        var ddl = tableTypeConverter.Convert(tableType.GeneratedTableType);
                        codeWriterTableType.WriteLine(ddl);
                        codeWriterTableType.WriteLine();

                        var fileName = _flywayFileNameService.GetFlywayFileName(tableType.GeneratedTableType);
                        //fileWriter.WriteFile(fileName, ddl);
                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations", fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(ddl);
                        }
                    }


                    foreach (var storedProc in databaseProject.StoredProcedure)
                    {
                        
                        var ddl = storedProcConverter.Convert(storedProc.GeneratedStoredProcedure);
                        codeWriterStoredProc.WriteLine(ddl);
                        codeWriter.WriteLine();

                        var fileName = _flywayFileNameService.GetFlywayFileName(storedProc.GeneratedStoredProcedure);
                        //fileWriter.WriteFile(fileName, ddl);
                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations", fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(ddl);
                        }
                    }
                }
                response.ZipAsBase64 =Convert.ToBase64String( memoryStream.ToArray());
            }
            response.ConvertedTableDDL = codeWriter.ToString();
            response.ConvertedTableTypeDDL = codeWriterTableType.ToString();
            response.ConvertedStoredProcedureDDL = codeWriterStoredProc.ToString();

            sw.Stop();
            GrpcEnvironment.Logger.Info($"Processed request in {sw.ElapsedMilliseconds} ms");
            return response;
        }

        

        public override async Task<QueryDatabaseTablesResponse> QueryDatabaseTables(QueryDatabaseTablesRequest request,
                ServerCallContext context)
        {

            var connectionString = _configuration.GetValue<string>("ConnectionStrings:CompanyPlatform");

            _sqlServerTablesReader.ConnectionString = connectionString;
            var dataTable = _sqlServerTablesReader.Read(request.Schema);

            var response = new QueryDatabaseTablesResponse();
            
            DatabaseTable lastTable = null;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                DatabaseTable table;
                if (lastTable !=null && (string)dataRow["table_schema"] == lastTable.SchemaName &&  (string)dataRow["table_name"] == lastTable.TableName)
                {
                    table = lastTable;
                }
                else
                {
                    table = new DatabaseTable();
                    lastTable = table;

                    table.SchemaName = (string) dataRow["table_schema"];
                    table.TableName = (string)dataRow["table_name"];
                    response.Tables.Add(table);
                }

                var column = new DatabaseColumn()
                {
                    ColumnName = (string) dataRow["column_name"]
                };

                table.Columns.Add(column);

            }

            var foreignKeys = _sqlServerTablesReader.ReadForeignKeys(request.Schema);

            foreach (var table in response.Tables)
            {
                foreach (var column in table.Columns)
                {
                    var fk = foreignKeys.FirstOrDefault(k =>
                        k.TableName.ToLower() == table.TableName.ToLower() && k.ColumnName.ToLower() == column.ColumnName.ToLower());

                    if (fk != null)
                    {
                        column.ForeignKeys.Add(new DatabaseForeignColumn() { SchemaName = fk.ReferencedTableSchema, TableName = fk.ReferencedTableName,  ColumnName = fk.ReferencedColumnName });
                    }
                }
            }
            return response;
        }

        public override async Task<SplitDDLResponse> SplitDDL(SplitDDLRequest request, ServerCallContext context)
        {
            var dataStoreType = DataStoreTypes.Unknown;
            if (request.DatabaseType == DatabaseTypes.SqlServer)
                dataStoreType = DataStoreTypes.SqlServer;
            else if (request.DatabaseType == DatabaseTypes.Postgres)
                dataStoreType = DataStoreTypes.Postgres;
            else
                throw new NotImplementedException();

            KDataStoreProject result = null;

            if (request.DatabaseType == DatabaseTypes.SqlServer)
            {
                var query = new SplitSqlServerDDLQuery()
                {
                    DataStoreType = DataStoreTypes.SqlServer,
                    UnSplitStoredProcedureDDL = request.UnSplitStoredProcedureDDL,
                    UnSplitTableDDL = request.UnSplitTableDDL,
                    UnSplitViewDDL =  request.UnSplitViewDDL,
                    UnSplitFunctionDDL =  request.UnSplitFunctionDDL,
                    UnSplitTableTypeDDL = request.UnSplitTableTypeDDL
                };
                result = await _executor.ExecuteAsync(query).ConfigureAwait(false);

            }
            else if (request.DatabaseType == DatabaseTypes.Postgres)
            {
                var query = new SplitPostgresDDLQuery()
                {
                    DataStoreType = DataStoreTypes.Postgres,
                    UnSplitStoredProcedureDDL = request.UnSplitStoredProcedureDDL,
                    UnSplitTableDDL = request.UnSplitTableDDL,
                    UnSplitViewDDL = request.UnSplitViewDDL,
                    UnSplitFunctionDDL = request.UnSplitFunctionDDL,
                    UnSplitTableTypeDDL = request.UnSplitTableTypeDDL
                };
                result = await _executor.ExecuteAsync(query).ConfigureAwait(false);
            }
            else 
                throw new NotImplementedException();

            var response = new SplitDDLResponse();
            using (var memoryStream = new MemoryStream())
            {
                using (var zipFile = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                {
                    foreach (var table in result.Table)
                    {
                        var fileName = _flywayFileNameService.GetFlywayFileName(new CTable(dataStoreType)
                        {
                            Schema = new CSchema { SchemaName = table.Schema.SchemaName },
                            TableName = table.TableName
                        });

                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations", fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(table.TableText);
                        }
                    }

                    foreach (var tableType in result.TableType)
                    {
                        var fileName = _flywayFileNameService.GetFlywayFileName(new CTableType(dataStoreType)
                        {
                            Schema = new CSchema { SchemaName = tableType.Schema },
                            TableName = tableType.TableTypeName
                        });

                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations", fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(tableType.TableTypeText);
                        }
                    }

                    foreach (var view in result.View)
                    {
                        var fileName = _flywayFileNameService.GetFlywayFileName(new CView()
                        {
                            Schema = new CSchema { SchemaName = view.Schema.SchemaName },
                            ViewName = view.ViewName
                        });

                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations", fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(view.ViewText);
                        }
                    }

                    foreach (var function in result.Function)
                    {
                        var fileName = _flywayFileNameService.GetFlywayFileName(new CFunction(dataStoreType)
                        {
                            Schema = new CSchema { SchemaName = function.Schema.SchemaName },
                            FunctionName = function.FunctionName
                        });

                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations", fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(function.FunctionText);
                        }
                    }


                    foreach (var storedProc in result.StoredProcedure)
                    {
                        var spFullName = storedProc.StoredProcedureName;
                        var schemaName = storedProc.Schema;
                        var spName = storedProc.StoredProcedureName;
                        var fileName = _flywayFileNameService.GetFlywayFileName(new CStoredProcedure(dataStoreType)
                        {
                            Schema = new CSchema { SchemaName = schemaName },
                            StoredProcedureName = spName
                        });

                        var newEntry = zipFile.CreateEntry(Path.Combine("migrations", fileName));
                        using (var writer = new StreamWriter(newEntry.Open()))
                        {
                            writer.Write(storedProc.StoredProcedureText);
                        }
                    }
                }
                response.ZipAsBase64 = Convert.ToBase64String(memoryStream.ToArray());
            }
            return response;
        }
        
        private string ConvertZipToBase64(string zipPath)
        {
            var bytes = File.ReadAllBytes(zipPath);
            return Convert.ToBase64String(bytes);
        }
    }

   
}
