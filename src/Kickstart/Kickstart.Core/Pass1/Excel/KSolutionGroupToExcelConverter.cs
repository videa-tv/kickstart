using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.CModel.Proto;
using SpreadsheetLight;
using Color = System.Drawing.Color;

namespace Kickstart.Pass1.Excel
{
    public interface IKSolutionGroupToExcelConverter
    {
        string OutputPath { get; set; }

        StreamWriter Convert(List<KSolutionGroup> solutionGroupList);
    }

    public class KSolutionGroupToExcelConverter : IKSolutionGroupToExcelConverter
    {
        private static readonly Random random = new Random();
        private readonly SLDocument _sl;
        private int _currentColumn;
        private int _currentRow;

        public KSolutionGroupToExcelConverter()
        {
            _sl = new SLDocument();
            _currentColumn = 1;
            _currentRow = 1;
        }

        public string OutputPath { get; set; }

        public StreamWriter Convert(List<KSolutionGroup> solutionGroupList)
        {
            CreateSolutionsSheet(solutionGroupList);
            CreateProtoSheet(solutionGroupList);
            CreateProtoRefSheet(solutionGroupList);
            CreateMockViewSheet(solutionGroupList);
            CreateTableTypeSheet(solutionGroupList);
            CreateStoredProcedureSheet(solutionGroupList);
            CreateStoredProcedureParameterSheet(solutionGroupList);
            CreateStoredProcedureResultSheet(solutionGroupList);


            var path = Path.GetDirectoryName(OutputPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            _sl.SaveAs(OutputPath);
            return null;
        }

        private void CreateProtoRefSheet(List<KSolutionGroup> solutionGroupList)
        {
            _currentColumn = 1;
            _currentRow = 1;

            _sl.AddWorksheet($"ProtoRef");
            _sl.FreezePanes(1, 0);

            _sl.SetColumnWidth(_currentColumn, 17);
            var colSolutionName = _currentColumn;
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.SolutionName));

            _sl.SetColumnWidth(_currentColumn, 30);
            var colServiceName = _currentColumn;
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CProtoService.ServiceName));

            _sl.SetColumnWidth(_currentColumn, 30);
            var colRpcName = _currentColumn;
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CProtoRpc.RpcName));

            _sl.SetColumnWidth(_currentColumn, 17);
            var colDirection = _currentColumn;
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CProtoRpcRef.Direction));
            _currentRow++;

            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            foreach (var project in solution.Project.Where(p => p is KGrpcProject).Select(p => p as KGrpcProject))
            {
                if (project.ProtoFile == null)
                    continue;
                var rowStyle = _sl.CreateStyle();

                rowStyle.Fill.SetPattern(PatternValues.Solid, GetRandomLightColor(), GetRandomLightColor());
                if (project is KGrpcIntegrationProject)
                {
                    var integrationProject = project as KGrpcIntegrationProject;
                    foreach (var protoRef in integrationProject.ProtoRef)
                    {
                        _sl.SetCellValue(_currentRow, colSolutionName, solution.SolutionName);
                        _sl.SetCellValue(_currentRow, colServiceName, protoRef.RefServiceName);

                        _sl.SetCellValue(_currentRow, colRpcName, protoRef.RefRpcName);

                        _sl.SetCellValue(_currentRow, colDirection, protoRef.Direction.ToString());


                        _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, rowStyle);
                        _currentRow++;
                    }
                }
            }
        }

        private void CreateProtoSheet(List<KSolutionGroup> solutionGroupList)
        {
            _currentColumn = 1;
            _currentRow = 1;

            _sl.AddWorksheet($"Proto");
            _sl.FreezePanes(1, 0);

            _sl.SetColumnWidth(_currentColumn, 17);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.SolutionName));

            _sl.SetColumnWidth(_currentColumn, 17);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CProtoFile.CSharpNamespace));

            _sl.SetColumnWidth(_currentColumn, 30);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CProtoService.ServiceName));

            _sl.SetColumnWidth(_currentColumn, 30);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CProtoRpc.RpcName));

            _sl.SetColumnWidth(_currentColumn, 30);
            _sl.SetCellValue(_currentRow, _currentColumn++, "RequestMessageName");

            _sl.SetColumnWidth(_currentColumn, 30);
            _sl.SetCellValue(_currentRow, _currentColumn++, "ResponseMessageName");

            _currentRow++;
            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            foreach (var project in solution.Project.Where(p => p is KGrpcProject).Select(p => p as KGrpcProject))
            {
                var rowStyle = _sl.CreateStyle();

                rowStyle.Fill.SetPattern(PatternValues.Solid, GetRandomLightColor(), GetRandomLightColor());
                if (project.ProtoFile == null)
                    continue;
                foreach (var protoFile in project.ProtoFile)
                foreach (var protoService in protoFile.GeneratedProtoFile.ProtoService)
                foreach (var rpc in protoService.Rpc)
                {
                    _currentColumn = 1;

                    _sl.SetCellValue(_currentRow, _currentColumn++, solution.SolutionName);

                    _sl.SetCellValue(_currentRow, _currentColumn++, protoService.ProtoFile.CSharpNamespace);

                    _sl.SetCellValue(_currentRow, _currentColumn++, protoService.ServiceName);

                    _sl.SetCellValue(_currentRow, _currentColumn++, rpc.RpcName);

                    _sl.SetCellValue(_currentRow, _currentColumn++, rpc.Request.MessageName);

                    _sl.SetCellValue(_currentRow, _currentColumn++, rpc.Response.MessageName);

                    _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, rowStyle);
                    _currentRow++;
                }
            }
            _sl.Filter(1, 1, _currentRow, _currentColumn);
        }

        private void CreateMockViewSheet(List<KSolutionGroup> solutionGroupList)
        {
            _currentColumn = 1;
            _currentRow = 1;

            _sl.AddWorksheet($"MockView");
            _sl.FreezePanes(1, 0);

            _sl.SetColumnWidth(_currentColumn, 17);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.SolutionName));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KView.ViewName));

            _sl.SetColumnWidth(_currentColumn, 30);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.ColumnName));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.ColumnSqlDbType));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.ColumnLength));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.IsPrimaryKey));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.IsNullable));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.IsUnique));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.IsIdentity));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.IsIndexed));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.IsCreatedDate));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.IsModifiedDate));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.IsRowVersion));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, "ForeignKeySchema");

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, "ForeignKeyTable");

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, "ForeignKeyColumn");
            //underline the headers
            var styleHeader = _sl.CreateStyle();
            styleHeader.SetBottomBorder(BorderStyleValues.Thick, Color.Black);

            _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, styleHeader);

            _currentRow++;

            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            foreach (var project in solution.Project
                .OfType<KDataStoreProject>())
            foreach (var mockView in project.MockView)
            {
                CView view = mockView;
                if (mockView.GeneratedView != null)
                    view = mockView.GeneratedView;

                var rowStyle = _sl.CreateStyle();

                rowStyle.Fill.SetPattern(PatternValues.Solid, GetRandomLightColor(), GetRandomLightColor());

                foreach (var column in view.Column)
                {
                    _currentColumn = 1;

                    _sl.SetCellValue(_currentRow, _currentColumn++, solution.SolutionName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, view.ViewName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.ColumnName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.ColumnSqlDbType.ToString());
                    _sl.SetCellValue(_currentRow, _currentColumn++,
                        column.ColumnLength == 0 ? string.Empty : column.ColumnLength.ToString());
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.IsPrimaryKey);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.IsNullable);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.IsUnique);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.IsIdentity);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.IsIndexed);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.IsCreatedDate);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.IsModifiedDate);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.IsRowVersion);

                    if (column.ForeignKeyColumn.Count > 0)
                    {
                        //todo: handle multiple
                        _sl.SetCellValue(_currentRow, _currentColumn++,
                            column.ForeignKeyColumn.First().View.Schema.SchemaName);
                        _sl.SetCellValue(_currentRow, _currentColumn++, column.ForeignKeyColumn.First().View.ViewName);
                        _sl.SetCellValue(_currentRow, _currentColumn++, column.ForeignKeyColumn.First().ColumnName);
                    }
                    _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, rowStyle);
                    _currentRow++;
                }
            }
            _sl.Filter(1, 1, _currentRow, _currentColumn);
        }

        private void CreateTableTypeSheet(List<KSolutionGroup> solutionGroupList)
        {
            _currentColumn = 1;
            _currentRow = 1;

            _sl.AddWorksheet($"TableTypes");
            _sl.FreezePanes(1, 0);

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.SolutionName));

            _sl.SetColumnWidth(_currentColumn, 10);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KTableType.Schema));


            _sl.SetColumnWidth(_currentColumn, 30);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KTableType.TableTypeName));

            _sl.SetColumnWidth(_currentColumn, 40);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.ColumnName));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.ColumnTypeRaw));

            _sl.SetColumnWidth(_currentColumn, 15);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.ColumnLength));
            //underline the headers
            var styleHeader = _sl.CreateStyle();
            styleHeader.SetBottomBorder(BorderStyleValues.Thick, Color.Black);

            _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, styleHeader);

            _currentRow++;

            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            {
                var rowStyle = _sl.CreateStyle();
                rowStyle.Fill.SetPattern(PatternValues.Solid, GetRandomLightColor(), GetRandomLightColor());
                foreach (var project in solution.Project.Where(p => p is KDataStoreProject)
                    .Select(p => p as KDataStoreProject))
                foreach (var tableType in project.TableType)
                    //var rowSubStyle = _sl.CreateStyle();
                    //rowSubStyle.Fill.SetPattern(PatternValues.Solid, GetRandomLightColor(), GetRandomLightColor());

                foreach (var column in tableType.GeneratedTableType.Column)
                {
                    _currentColumn = 1;

                    _sl.SetCellValue(_currentRow, _currentColumn++, solution.SolutionName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, tableType.Schema);
                    _sl.SetCellValue(_currentRow, _currentColumn++, tableType.GeneratedTableType.TableName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.ColumnName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.ColumnTypeRaw);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.ColumnLength);

                    //_sl.SetCellStyle(_currentRow, 1, _currentRow, 1, rowStyle);
                    _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, rowStyle);
                    _currentRow++;
                }
            }
            _sl.Filter(1, 1, _currentRow, _currentColumn);
        }


        private void CreateStoredProcedureSheet(List<KSolutionGroup> solutionGroupList)
        {
            _currentColumn = 1;
            _currentRow = 1;

            _sl.AddWorksheet($"StoredProcedures");
            _sl.FreezePanes(1, 0);

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.SolutionName));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KStoredProcedure.Schema));

            _sl.SetColumnWidth(_currentColumn, 40);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KStoredProcedure.StoredProcedureName));

            _sl.SetColumnWidth(_currentColumn, 40);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KStoredProcedure.ParameterSetName));

            _sl.SetColumnWidth(_currentColumn, 40);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KStoredProcedure.ResultSetName));

            _sl.SetColumnWidth(_currentColumn, 40);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KStoredProcedure.ReturnsMultipleRows));
            // _sl.SetColumnWidth(_currentColumn, 20);
            //_sl.SetCellValue(_currentRow, _currentColumn++, nameof(MStoredProcedure.ColumnName));

            //underline the headers
            var styleHeader = _sl.CreateStyle();
            styleHeader.SetBottomBorder(BorderStyleValues.Thick, Color.Black);

            _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, styleHeader);

            _currentRow++;

            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            {
                var rowStyle = _sl.CreateStyle();
                rowStyle.Fill.SetPattern(PatternValues.Solid, GetRandomLightColor(), GetRandomLightColor());
                foreach (var project in solution.Project.Where(p => p is KDataStoreProject)
                    .Select(p => p as KDataStoreProject))
                foreach (var storedProc in project.StoredProcedure)
                {
                    _currentColumn = 1;

                    _sl.SetCellValue(_currentRow, _currentColumn++, solution.SolutionName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, storedProc.Schema);
                    _sl.SetCellValue(_currentRow, _currentColumn++, storedProc.StoredProcedureName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, storedProc.ParameterSetName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, storedProc.ResultSetName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, storedProc.ReturnsMultipleRows);
                    _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, rowStyle);
                    _currentRow++;
                }
            }
            _sl.Filter(1, 1, _currentRow, _currentColumn);
        }

        private void CreateStoredProcedureParameterSheet(List<KSolutionGroup> solutionGroupList)
        {
            _currentColumn = 1;
            _currentRow = 1;

            _sl.AddWorksheet($"StoredProcedureParameters");
            _sl.FreezePanes(1, 0);

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.SolutionName));

            _sl.SetColumnWidth(_currentColumn, 40);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KStoredProcedure.StoredProcedureName));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CStoredProcedureParameter.ParameterName));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CStoredProcedureParameter.ParameterType));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CStoredProcedureParameter.ParameterLength));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++,
                nameof(CStoredProcedureParameter.ParameterTypeIsUserDefined));
            //underline the headers
            var styleHeader = _sl.CreateStyle();
            styleHeader.SetBottomBorder(BorderStyleValues.Thick, Color.Black);

            _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, styleHeader);

            _currentRow++;
            var random = new Random();

            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            foreach (var project in solution.Project.Where(p => p is KDataStoreProject)
                .Select(p => p as KDataStoreProject))
            foreach (var storedProc in project.StoredProcedure)
            {
                var rowStyle = _sl.CreateStyle();


                rowStyle.Fill.SetPattern(PatternValues.Solid, GetRandomLightColor(), GetRandomLightColor());
                foreach (var parameter in storedProc.GeneratedStoredProcedure.Parameter)
                {
                    _currentColumn = 1;

                    _sl.SetCellValue(_currentRow, _currentColumn++, solution.SolutionName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, storedProc.StoredProcedureName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, parameter.ParameterName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, parameter.ParameterTypeRaw);
                    _sl.SetCellValue(_currentRow, _currentColumn++,
                        parameter.ParameterLength == 0 ? string.Empty : parameter.ParameterLength.ToString());
                    _sl.SetCellValue(_currentRow, _currentColumn++, parameter.ParameterTypeIsUserDefined);


                    _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, rowStyle);
                    _currentRow++;
                }
            }
            _sl.Filter(1, 1, _currentRow, _currentColumn);
        }

        private Color GetRandomLightColor()
        {
            var r = random.Next() % 255 / 3 + 168;
            var g = random.Next() % 255 / 3 + 168;
            var b = random.Next() % 255 / 3 + 168;
            return Color.FromArgb(r, g, b);
        }

        private void CreateStoredProcedureResultSheet(List<KSolutionGroup> solutionGroupList)
        {
            _currentColumn = 1;
            _currentRow = 1;

            _sl.AddWorksheet($"StoredProcedureResult");
            _sl.FreezePanes(1, 0);

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.SolutionName));

            _sl.SetColumnWidth(_currentColumn, 40);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KStoredProcedure.StoredProcedureName));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(CColumn.ColumnName));

            //underline the headers
            var styleHeader = _sl.CreateStyle();
            styleHeader.SetBottomBorder(BorderStyleValues.Thick, Color.Black);

            _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, styleHeader);

            _currentRow++;

            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            foreach (var project in solution.Project.Where(p => p is KDataStoreProject)
                .Select(p => p as KDataStoreProject))
            foreach (var storedProc in project.StoredProcedure)
            {
                var rowStyle = _sl.CreateStyle();
                rowStyle.Fill.SetPattern(PatternValues.Solid, GetRandomLightColor(), GetRandomLightColor());
                foreach (var column in storedProc.GeneratedStoredProcedure.ResultSet)
                {
                    _currentColumn = 1;

                    _sl.SetCellValue(_currentRow, _currentColumn++, solution.SolutionName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, storedProc.StoredProcedureName);
                    _sl.SetCellValue(_currentRow, _currentColumn++, column.ColumnName);

                    _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, rowStyle);
                    _currentRow++;
                }
            }
            _sl.Filter(1, 1, _currentRow, _currentColumn);
        }

        private void CreateSolutionsSheet(List<KSolutionGroup> solutionGroupList)
        {
            _currentColumn = 1;
            _currentRow = 1;

            _sl.RenameWorksheet(SLDocument.DefaultFirstSheetName, "Solutions");
            //print headers
            _sl.FreezePanes(1, 0);


            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolutionGroup.SolutionGroupName));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.CompanyName));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KSolution.SolutionName));
            /*
            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.KickstartdatabaseProject));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.DbSuffix));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.DefaultSchemaName));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.KickstartAuditTables));


            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.KickstartCRUDStoredProcedures));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.KickstartTableSeedScripts));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.KickstartDataProject));


            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.KickstartGrpcProjectFlag));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(MSolution.KickstartGrpcIntegrationProjectFlag));
            */
            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KDataStoreProject.SqlViewFile));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KDataStoreProject.MockSqlViewFile));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KDataStoreProject.SqlTableTypeFile));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KDataStoreProject.SqlStoredProcedureFile));

            _sl.SetColumnWidth(_currentColumn, 20);
            _sl.SetCellValue(_currentRow, _currentColumn++, nameof(KProtoFile.ProtoFileFile));

            //underline the headers
            var styleHeader = _sl.CreateStyle();
            styleHeader.SetBottomBorder(BorderStyleValues.Thick, Color.Black);

            _sl.SetCellStyle(_currentRow, 1, _currentRow, _currentColumn, styleHeader);
            _currentRow++;


            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            foreach (var project in solution.Project.Where(p => p is KDataStoreProject)
                .Select(p => p as KDataStoreProject))
            {
                _currentColumn = 1;

                _sl.SetCellValue(_currentRow, _currentColumn++, solutionGroup.SolutionGroupName);

                _sl.SetCellValue(_currentRow, _currentColumn++, solution.CompanyName);

                _sl.SetCellValue(_currentRow, _currentColumn++, solution.SolutionName);
                /*
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.KickstartdatabaseProject);
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.DbSuffix);
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.DefaultSchemaName);
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.KickstartAuditTables);
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.KickstartCRUDStoredProcedures);
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.KickstartTableSeedScripts);
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.KickstartDataProject);
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.KickstartGrpcProjectFlag);
                        _sl.SetCellValue(_currentRow, _currentColumn++, solution.KickstartGrpcIntegrationProjectFlag);
                        */
                _sl.SetCellValue(_currentRow, _currentColumn++, project.SqlViewFile);
                _sl.SetCellValue(_currentRow, _currentColumn++, project.MockSqlViewFile);
                _sl.SetCellValue(_currentRow, _currentColumn++, project.SqlTableTypeFile);
                _sl.SetCellValue(_currentRow, _currentColumn++, project.SqlStoredProcedureFile);
                //_sl.SetCellValue(_currentRow, _currentColumn++, project.ProtoFileFile);
                _currentRow++;
            }
            _sl.Filter(1, 1, _currentRow, _currentColumn);
        }
    }
}