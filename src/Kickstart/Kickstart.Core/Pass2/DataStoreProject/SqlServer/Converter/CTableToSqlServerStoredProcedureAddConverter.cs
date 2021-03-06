using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.SqlServer
{
    public class CTableToSqlServerStoredProcedureAddConverter
    {
        #region Methods

        public CStoredProcedure Convert(CTable table)
        {
            var storedProcedure = new CStoredProcedure( DataStoreTypes.SqlServer);
            storedProcedure.Schema = new CSchema {SchemaName = $"{table.Schema.SchemaName}Api"};
            storedProcedure.StoredProcedureName = $"{table.TableName}Add";
            storedProcedure.ResultSetName = table.TableName;
            storedProcedure.Parameter = new List<CStoredProcedureParameter>();
            foreach (var column in table.Column)
            {
                if (column.IsIdentity)
                    continue;

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
            stringBuilder.AppendLine($"INSERT INTO [{table.Schema.SchemaName}].[{table.TableName}] ");
            stringBuilder.Append("(");
            var first = true;
            foreach (var column in table.Column)
            {
                if (column.IsIdentity)
                    continue;
                if (!first)
                    stringBuilder.Append("\t\t,");

                stringBuilder.AppendLine(column.ColumnName);
                first = false;
            }
            stringBuilder.AppendLine(")");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.Append("(");
            first = true;
            foreach (var parameter in storedProcedure.Parameter)
            {
                if (!first)
                    stringBuilder.Append("\t\t,");
                first = false;
                stringBuilder.AppendLine(parameter.ParameterName);
            }
            stringBuilder.AppendLine(")");
            stringBuilder.AppendLine();

            var identityCol = table.Column.FirstOrDefault(c => c.IsIdentity);
            if (identityCol != null)
                stringBuilder.AppendLine(
                    $"SELECT CAST(SCOPE_IDENTITY() as {SqlMapper.DbTypeToSqlDbType(identityCol.ColumnType)})");
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