using System.Collections.Generic;
using System.Text;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.SqlServer
{
    public interface ICTableToCStoredProcedureUpdateConverter
    {
        CStoredProcedure Convert(CTable table);
    }

    public class CTableToCStoredProcedureUpdateConverter : ICTableToCStoredProcedureUpdateConverter
    {
        #region Methods

        public CStoredProcedure Convert(CTable table)
        {
            var storedProcedure = new CStoredProcedure( DataStoreTypes.SqlServer);
            storedProcedure.Schema = new CSchema {SchemaName = $"{table.Schema.SchemaName}Api"};
            storedProcedure.StoredProcedureName = $"{table.TableName}Update";
            storedProcedure.ResultSetName = table.TableName;
            storedProcedure.Parameter = new List<CStoredProcedureParameter>();
            foreach (var column in table.Column)
            {
                var parameter = new CStoredProcedureParameter
                {
                    ParameterName = $"{column.ColumnName}",
                    ParameterType = column.ColumnType,
                    ParameterTypeRaw = column.ColumnTypeRaw,
                    ParameterLength = column.ColumnLength,
                    SourceColumn = column
                };
                storedProcedure.Parameter.Add(parameter);
            }
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"UPDATE [{table.Schema.SchemaName}].[{table.TableName}] ");
            stringBuilder.AppendLine("SET");

            var pkColumns = table.GetPrimaryKeyColumns();
            var first = true;
            foreach (var column in table.Column)
            {
                if (column.IsIdentity)
                    continue;

                if (pkColumns.Contains(column))
                    continue;

                if (!first)
                    stringBuilder.Append("\t\t,");

                stringBuilder.AppendLine($"{column.ColumnName} = @{column.ColumnName}");
                first = false;
            }
            stringBuilder.AppendLine("WHERE");
            first = true;
            foreach (var pkColumn in pkColumns)
            {
                if (!first)
                    stringBuilder.Append(" AND ");
                stringBuilder.AppendLine($"\t\t{pkColumn.ColumnName} = @{pkColumn.ColumnName}");
            }

            storedProcedure.StoredProcedureBody = stringBuilder.ToString();
            return storedProcedure;
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