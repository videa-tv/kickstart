using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using SpreadsheetLight;

namespace Kickstart.Pass1.Excel
{
    public class ExcelToKSolutionGroupConverter : IExcelToKSolutionGroupConverter
    {
        private SLDocument _sl;

        public List<KSolutionGroup> Convert(string excelFilePath)
        {
            _sl = new SLDocument(excelFilePath);

            var solutionGroupList = GetSolutionGroups();
            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            {
                foreach (var project in solution.Project.Where(p => p is KDataStoreProject)
                    .Select(p => p as KDataStoreProject))
                {
                    project.MockView.AddRange(GetMockViews(solution.SolutionName));
                    project.TableType.AddRange(GetTableTypes(solution.SolutionName));
                    project.StoredProcedure.AddRange(GetStoredProcedures(solution.SolutionName));
                }
                foreach (var project in solution.Project.Where(p => p is KGrpcProject).Select(p => p as KGrpcProject))
                    project.ProtoFile.Add(GetProtoFile(solution.SolutionName));
            }
            return solutionGroupList;
        }

        private KProtoFile GetProtoFile(string solutionName)
        {
            _sl.SelectWorksheet("Proto");

            var colSolutionName = GetColumnIndex("SolutionName");
            var colCSharpNamespace = GetColumnIndex("CSharpNamespace");
            var colServiceName = GetColumnIndex("ServiceName");
            var colRpcName = GetColumnIndex("RpcName");
            var colRequestMessageName = GetColumnIndex("RequestMessageName");
            var colResponseMessageName = GetColumnIndex("ResponseMessageName");

            var currentRow = 2;
            var protoFile = new CProtoFile();
            while (!string.IsNullOrEmpty(_sl.GetCellValueAsString(currentRow, colSolutionName)))
            {
                if (_sl.GetCellValueAsString(currentRow, colSolutionName) != solutionName)
                {
                    currentRow++;
                    continue;
                }
                protoFile.CSharpNamespace = _sl.GetCellValueAsString(currentRow, colCSharpNamespace);

                var protoService = new CProtoService(protoFile)
                {
                    ServiceName = _sl.GetCellValueAsString(currentRow, colServiceName)
                };

                protoFile.ProtoService.Add(protoService);
                var rpc = new CProtoRpc(protoService) {RpcName = _sl.GetCellValueAsString(currentRow, colRpcName)};
                rpc.Request = new CProtoMessage (rpc)
                {
                    MessageName = _sl.GetCellValueAsString(currentRow, colRequestMessageName)
                };
                rpc.Response = new CProtoMessage (rpc)
                {
                    MessageName = _sl.GetCellValueAsString(currentRow, colResponseMessageName)
                };


                protoService.Rpc.Add(rpc);
                /*
                    while (_sl.GetCellValueAsString(currentRow, colServiceName) == view.ViewName)
                    {
                        var column = new SColumn(view)
                        {
                            ColumnName = _sl.GetCellValueAsString(currentRow, colColumnName),
                            ColumnTypeRaw = _sl.GetCellValueAsString(currentRow, colColumnSqlDbType),
                        };
                        view.Column.Add(column);
                        currentRow++;
                    }
                    */
                currentRow++;
            }
            return new KProtoFile {GeneratedProtoFile = protoFile};
        }

        private List<KSolutionGroup> GetSolutionGroups()
        {
            var solutionGroupList = new List<KSolutionGroup>();
            _sl.SelectWorksheet("Solutions");


            var colSolutionGroup = GetColumnIndex("SolutionGroupName");
            var colSolutionName = GetColumnIndex("SolutionName");
            var colCompanyName = GetColumnIndex("CompanyName");
            var colDefaultSchemaName = GetColumnIndex("DefaultSchemaName");

            var colKickstartdatabaseProject = GetColumnIndex("KickstartdatabaseProject");
            var colKickstartDataProject = GetColumnIndex("KickstartDataProject");
            var colKickstartAuditTables = GetColumnIndex("KickstartAuditTables");
            var colKickstartCRUDStoredProcedures = GetColumnIndex("KickstartCRUDStoredProcedures");
            var colKickstartTableSeedScripts = GetColumnIndex("KickstartTableSeedScripts");

            var colKickstartGrpcProjectFlag = GetColumnIndex("KickstartGrpcProjectFlag");
            var colKickstartGrpcIntegrationProjectFlag = GetColumnIndex("KickstartGrpcIntegrationProjectFlag");

            var colSqlViewFile = GetColumnIndex("SqlViewFile");
            /*
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            var col = GetColumnIndex("");
            */

            var currentRow = 2;
            var solutionGroup =
                new KSolutionGroup {SolutionGroupName = _sl.GetCellValueAsString(currentRow, colSolutionGroup)};
            solutionGroupList.Add(solutionGroup);
            while (!string.IsNullOrEmpty(_sl.GetCellValueAsString(currentRow, colSolutionGroup)))
            {
                if (solutionGroup.SolutionGroupName != _sl.GetCellValueAsString(currentRow, colSolutionGroup))
                {
                    solutionGroup = new KSolutionGroup
                    {
                        SolutionGroupName = _sl.GetCellValueAsString(currentRow, colSolutionGroup)
                    };
                    solutionGroupList.Add(solutionGroup);
                }
                var solution = new KSolution
                {
                    SolutionName = _sl.GetCellValueAsString(currentRow, colSolutionName),
                    CompanyName = _sl.GetCellValueAsString(currentRow, colCompanyName)
                    /*
                    DefaultSchemaName = _sl.GetCellValueAsString(currentRow, colDefaultSchemaName),
                    KickstartdatabaseProject = _sl.GetCellValueAsBoolean(currentRow, colKickstartdatabaseProject),
                    KickstartDataProject = _sl.GetCellValueAsBoolean(currentRow, colKickstartDataProject),
                    KickstartAuditTables = _sl.GetCellValueAsBoolean(currentRow, colKickstartAuditTables),
                    KickstartCRUDStoredProcedures = _sl.GetCellValueAsBoolean(currentRow, colKickstartCRUDStoredProcedures),
                    KickstartTableSeedScripts = _sl.GetCellValueAsBoolean(currentRow, colKickstartTableSeedScripts),
                   
                    KickstartGrpcProjectFlag = _sl.GetCellValueAsBoolean(currentRow, colKickstartGrpcProjectFlag),
                    KickstartGrpcIntegrationProjectFlag = _sl.GetCellValueAsBoolean(currentRow, colKickstartGrpcIntegrationProjectFlag),
                    */
                };
                var databaseProject = new KDataStoreProject();
                databaseProject.SqlViewFile = _sl.GetCellValueAsString(currentRow, colSqlViewFile);
                solution.Project.Add(databaseProject);

                solution.GeneratedSolution = new CSolution {SolutionName = solution.SolutionName};
                solutionGroup.Solution.Add(solution);

                currentRow++;
            }
            return solutionGroupList;
        }

        private IEnumerable<KView> GetMockViews(string solutionName)
        {
            _sl.SelectWorksheet("MockView");
            var views = new List<KView>();
            var colSolutionName = GetColumnIndex("SolutionName");
            var colViewName = GetColumnIndex("ViewName");
            var colColumnName = GetColumnIndex("ColumnName");
            var colColumnSqlDbType = GetColumnIndex("ColumnSqlDbType");
            var colColumnLength = GetColumnIndex("ColumnLength");
            var colIsPrimaryKey = GetColumnIndex("IsPrimaryKey");
            var colIsNullable = GetColumnIndex("IsNullable");
            var colIsUnique = GetColumnIndex("IsUnique");
            var colIsIdentity = GetColumnIndex("IsIdentity");
            var colIsIndexed = GetColumnIndex("IsIndexed");
            var colIsCreatedDate = GetColumnIndex("IsCreatedDate");
            var colIsModifiedDate = GetColumnIndex("IsModifiedDate");
            var colIsRowVersion = GetColumnIndex("IsRowVersion");
            var colForeignKeySchema = GetColumnIndex("ForeignKeySchema");
            var colForeignKeyTable = GetColumnIndex("ForeignKeyTable");
            var colForeignKeyColumn = GetColumnIndex("ForeignKeyColumn");

            var currentRow = 2;
            while (!string.IsNullOrEmpty(_sl.GetCellValueAsString(currentRow, colSolutionName)))
            {
                if (_sl.GetCellValueAsString(currentRow, colSolutionName) != solutionName)
                {
                    currentRow++;
                    continue;
                }
                var view = new KView {ViewName = _sl.GetCellValueAsString(currentRow, colViewName)};
                while (_sl.GetCellValueAsString(currentRow, colViewName) == view.ViewName)
                {
                    var column = new CColumn(view)
                    {
                        ColumnName = _sl.GetCellValueAsString(currentRow, colColumnName),
                        ColumnTypeRaw = _sl.GetCellValueAsString(currentRow, colColumnSqlDbType),
                        ColumnLength = _sl.GetCellValueAsInt32(currentRow, colColumnLength),
                        IsPrimaryKey = _sl.GetCellValueAsBoolean(currentRow, colIsPrimaryKey),
                        IsNullable = _sl.GetCellValueAsBoolean(currentRow, colIsNullable),
                        IsUnique = _sl.GetCellValueAsBoolean(currentRow, colIsUnique),
                        IsIdentity = _sl.GetCellValueAsBoolean(currentRow, colIsIdentity),
                        IsIndexed = _sl.GetCellValueAsBoolean(currentRow, colIsIndexed)
                        //IsCreatedDate=_sl.GetCellValueAsBoolean(currentRow, colIsCreatedDate),
                        //IsModifiedDate = _sl.GetCellValueAsBoolean(currentRow, colIsModifiedDate),
                        //IsRowVersion = _sl.GetCellValueAsBoolean(currentRow, colIsRowVersion),
                    };


                    view.Column.Add(column);
                    currentRow++;
                }
                views.Add(view);
                currentRow++;
            }
            //now that all the views have been added, parse the foreign keys
            while (!string.IsNullOrEmpty(_sl.GetCellValueAsString(currentRow, colSolutionName)))
            {
                if (_sl.GetCellValueAsString(currentRow, colSolutionName) != solutionName)
                {
                    currentRow++;
                    continue;
                }
                var viewName = _sl.GetCellValueAsString(currentRow, colViewName);
                var view = views.First(v => v.ViewName == viewName);

                while (_sl.GetCellValueAsString(currentRow, colViewName) == viewName)
                {
                    var columnName = _sl.GetCellValueAsString(currentRow, colColumnName);

                    var foreignKeySchema = _sl.GetCellValueAsString(currentRow, colForeignKeySchema);
                    var foreignKeyTable = _sl.GetCellValueAsString(currentRow, colForeignKeyTable);
                    var foreignKeyColumn = _sl.GetCellValueAsString(currentRow, colForeignKeyColumn);

                    var foreignKeyView = views.First(v => v.ViewName == foreignKeyTable);
                    var foreignKeyCol = foreignKeyView.Column.FirstOrDefault(c => c.ColumnName == foreignKeyColumn);

                    var col = view.Column.FirstOrDefault(c => c.ColumnName == columnName);
                    col.ForeignKeyColumn.Add(foreignKeyCol);
                }
            }
            return views;
        }

        private IEnumerable<KTableType> GetTableTypes(string solutionName)
        {
            _sl.SelectWorksheet("TableTypes");
            var tableTypes = new List<KTableType>();
            var colSolutionName = GetColumnIndex("SolutionName");
            var colSchema = GetColumnIndex("Schema");
            var colTableTypeName = GetColumnIndex("TableTypeName");
            var colColumnName = GetColumnIndex("ColumnName");
            var colColumnTypeRaw = GetColumnIndex("ColumnTypeRaw");

            var colColumnLength = GetColumnIndex("ColumnLength");
            var currentRow = 2;
            while (!string.IsNullOrEmpty(_sl.GetCellValueAsString(currentRow, colSolutionName)))
            {
                if (_sl.GetCellValueAsString(currentRow, colSolutionName) != solutionName)
                {
                    currentRow++;
                    continue;
                }
                var tableType = new KTableType
                {
                    Schema = _sl.GetCellValueAsString(currentRow, colSchema),
                    TableTypeName = _sl.GetCellValueAsString(currentRow, colTableTypeName)
                };
                tableType.GeneratedTableType = new CTableType (Utility.DataStoreTypes.Unknown)
                {
                    TableName = tableType.TableTypeName,
                    Schema = new CSchema {SchemaName = tableType.Schema}
                };
                while (tableType.TableTypeName == _sl.GetCellValueAsString(currentRow, colTableTypeName))
                {
                    var column = new CColumn(tableType.GeneratedTableType)
                    {
                        ColumnName = _sl.GetCellValueAsString(currentRow, colColumnName),
                        ColumnTypeRaw = _sl.GetCellValueAsString(currentRow, colColumnTypeRaw),
                        ColumnLength = _sl.GetCellValueAsInt32(currentRow, colColumnLength)
                    };
                    tableType.GeneratedTableType.Column.Add(column);
                    currentRow++;
                }

                //tableType.GeneratedTableType.Parameter.AddRange(GetStoredProcedureParameters(storedProcedure.StoredProcedureName));
                tableTypes.Add(tableType);
                // currentRow++;
            }
            return tableTypes;
        }

        private IEnumerable<KStoredProcedure> GetStoredProcedures(string solutionName)
        {
            _sl.SelectWorksheet("StoredProcedures");
            var storedProcedures = new List<KStoredProcedure>();
            var colSolutionName = GetColumnIndex("SolutionName");
            var colSchema = GetColumnIndex("Schema");
            var colStoredProcedureName = GetColumnIndex("StoredProcedureName");
            var colResultSetName = GetColumnIndex("ResultSetName");
            var colReturnsMultipleRows = GetColumnIndex("ReturnsMultipleRows");
            var currentRow = 2;
            while (!string.IsNullOrEmpty(_sl.GetCellValueAsString(currentRow, colSolutionName)))
            {
                if (_sl.GetCellValueAsString(currentRow, colSolutionName) != solutionName)
                {
                    currentRow++;
                    continue;
                }
                var storedProcedure = new KStoredProcedure
                {
                    Schema = _sl.GetCellValueAsString(currentRow, colSchema),
                    StoredProcedureName = _sl.GetCellValueAsString(currentRow, colStoredProcedureName),
                    ResultSetName = _sl.GetCellValueAsString(currentRow, colResultSetName),
                    ReturnsMultipleRows = _sl.GetCellValueAsBoolean(currentRow, colReturnsMultipleRows)
                };
               // storedProcedure.GeneratedStoredProcedure =
                 //   new CStoredProcedure {StoredProcedureName = storedProcedure.StoredProcedureName};
                storedProcedure.GeneratedStoredProcedure.Schema = new CSchema {SchemaName = storedProcedure.Schema};


                storedProcedures.Add(storedProcedure);
                currentRow++;
            }
            foreach (var storedProcedure in storedProcedures)
                storedProcedure.GeneratedStoredProcedure.Parameter.AddRange(
                    GetStoredProcedureParameters(solutionName, storedProcedure.StoredProcedureName));
            return storedProcedures;
        }

        private IEnumerable<CStoredProcedureParameter> GetStoredProcedureParameters(string solutionName,
            string storedProcedureName)
        {
            _sl.SelectWorksheet("StoredProcedureParameters");
            var parameters = new List<CStoredProcedureParameter>();
            var colSolutionName = GetColumnIndex("SolutionName");
            var colStoredProcedureName = GetColumnIndex("StoredProcedureName");
            var colParameterName = GetColumnIndex("ParameterName");
            var colParameterType = GetColumnIndex("ParameterType");
            var colParameterTypeIsUserDefined = GetColumnIndex("ParameterTypeIsUserDefined");
            var currentRow = 2;
            while (!string.IsNullOrEmpty(_sl.GetCellValueAsString(currentRow, colStoredProcedureName)))
            {
                if (_sl.GetCellValueAsString(currentRow, colSolutionName) != solutionName)
                {
                    currentRow++;
                    continue;
                }

                if (_sl.GetCellValueAsString(currentRow, colStoredProcedureName) != storedProcedureName)
                {
                    currentRow++;
                    continue;
                }
                var storedProcedureParameter = new CStoredProcedureParameter
                {
                    ParameterName = _sl.GetCellValueAsString(currentRow, colParameterName),
                    ParameterTypeRaw = _sl.GetCellValueAsString(currentRow, colParameterType),
                    ParameterTypeIsUserDefined = _sl.GetCellValueAsBoolean(currentRow, colParameterTypeIsUserDefined)
                };
                parameters.Add(storedProcedureParameter);
                currentRow++;
            }
            return parameters;
        }

        private int GetColumnIndex(string columnName)
        {
            var headerRow = 1;
            var currentCol = 1;
            while (!string.IsNullOrWhiteSpace(_sl.GetCellValueAsString(headerRow, currentCol)))
            {
                if (_sl.GetCellValueAsString(headerRow, currentCol) == columnName)
                    return currentCol;
                currentCol++;
            }
            return -1;
        }
    }
}