using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.DataStore;
using SpreadsheetLight;

namespace Kickstart.Pass1.Excel
{
    internal class ExcelToRowDataConverter
    {
        public IList<CTableRow> Convert(KView kView)
        {
            var rows = new List<CTableRow>();

            var sl = OpenSpreadsheet(kView);
            //var sl = new SLDocument((kView.SampleDataExcelFile);
            //sl.SelectWorksheet("");

            var colPrimaryKey = kView.GeneratedView.Column.First(c => c.IsPrimaryKey);
            var colPrimaryKeyIndex = GetColumnIndex(sl, colPrimaryKey.ColumnName);

            var currentRow = 2;
            while (!string.IsNullOrEmpty(sl.GetCellValueAsString(currentRow, colPrimaryKeyIndex)))
            {
                var row = new CTableRow();
                foreach (var viewCol in kView.GeneratedView.Column)
                {
                    var col = GetColumnIndex(sl, viewCol.ColumnName);
                    if (col < 1)
                        continue;
                    var value = sl.GetCellValueAsString(currentRow, col);
                    row.RowData.Add(new CTableRowData {Column = viewCol, Value = value});
                }
                currentRow++;
                rows.Add(row);
            }


            return rows;
        }

        private SLDocument OpenSpreadsheet(KView kView)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = kView.SampleDataExcelFile;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;
                return new SLDocument(stream);
            }
        }

        private int GetColumnIndex(SLDocument sl, string columnName)
        {
            var headerRow = 1;
            var currentCol = 1;
            while (!string.IsNullOrWhiteSpace(sl.GetCellValueAsString(headerRow, currentCol)))
            {
                if (sl.GetCellValueAsString(headerRow, currentCol) == columnName)
                    return currentCol;
                currentCol++;
            }
            return -1;
        }
    }
}