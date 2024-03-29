using System.Data;
using System.Linq;
using System.Text;
using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass2.SqlServer
{
    public class CTableToSSeedScriptConverter
    {
        #region Methods

        public CSeedScript Convert(CTable table)
        {
            var seedScript = new CSeedScript {SeedScriptName = $"Seed{table.TableName}"};
            var stringBuilder = new StringBuilder();

            foreach (var row in table.Row)
            {
                stringBuilder.AppendLine(
                    $"EXEC [{table.InsertStoredProcedure.Schema.SchemaName}].[{table.InsertStoredProcedure.StoredProcedureName}]");

                var first = true;
                foreach (var rowData in row.RowData)
                {
                    // if (rowData.Column.IsIdentity)
                    //   continue;

                    if (!first)
                        stringBuilder.Append(", ");

                    var parameter = table.InsertStoredProcedure.Parameter.FirstOrDefault(p =>
                        p.SourceColumn.ColumnName == rowData.Column.ColumnName);
                    var value = "NULL";
                    if (rowData.Column.ColumnType == DbType.String ||
                        rowData.Column.ColumnType == DbType.StringFixedLength ||
                        rowData.Column.ColumnType == DbType.AnsiString ||
                        rowData.Column.ColumnType == DbType.AnsiStringFixedLength)
                        value = $"'{rowData.Value}'";
                    else
                        value = $"{rowData.Value}";
                    stringBuilder.Append($"@{parameter.ParameterName} = {value}");
                    first = false;
                }
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("GO");
                stringBuilder.AppendLine();
            }

            seedScript.SeedScriptBody = stringBuilder.ToString();
            return seedScript;
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}