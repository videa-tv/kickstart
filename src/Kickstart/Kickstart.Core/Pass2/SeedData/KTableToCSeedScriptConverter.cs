using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.SeedData
{
    public class KTableToCSeedScriptConverter
    {
        public CSeedScript Convert(KTable kTable)
        {
            var seedScript = new CSeedScript();
            var codeWriter = new CodeWriter();
            if (kTable.GeneratedTable.Row.Count == 0)
                codeWriter.WriteLine($"-- Sample data has not been supplied for {kTable.GeneratedTable.TableName} ");
            var firstRow = true;

            foreach (var row in kTable.GeneratedTable.Row)
            {
                if (!firstRow)
                    codeWriter.WriteLine("UNION ALL");
                codeWriter.Write("INSERT INTO (");
                var firstColumnHeader = true;
                foreach (var column in kTable.GeneratedTable.Column)
                {
                    if (!firstColumnHeader)
                        codeWriter.Write(", ");
                    codeWriter.Write($"[{column.ColumnName}]");

                    firstColumnHeader = false;
                }
                codeWriter.Write(") VALUES (");

                var firstColumnValue = true;
                foreach (var column in kTable.GeneratedTable.Column)
                {
                    if (!firstColumnValue)
                        codeWriter.Write(", ");

                    var data = row.RowData.FirstOrDefault(rd => rd.Column.ColumnName == column.ColumnName);
                    if (data != null)
                        codeWriter.Write($"{data}");
                    else
                        codeWriter.Write("NULL");
                    firstColumnValue = false;
                }
                codeWriter.WriteLine(")");

                firstRow = false;
            }

            seedScript.SeedScriptBody = codeWriter.ToString();
            return seedScript;
        }
    }
}