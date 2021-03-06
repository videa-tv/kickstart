using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.SqlServer
{
    public interface ICTableToSqlServerStoredProcedureAddUpdateConverter
    {
        CStoredProcedure Convert(CTable table);
    }

    public class CTableToSqlServerStoredProcedureAddUpdateConverter : ICTableToSqlServerStoredProcedureAddUpdateConverter
    {
        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors

        #region Methods

        public CStoredProcedure Convert(CTable table)
        {
            //http://michaeljswart.com/2011/09/mythbusting-concurrent-updateinsert-solutions/

            var storedProcedure = new CStoredProcedure(DataStoreTypes.SqlServer)
            {
                Schema = new CSchema {SchemaName = $"{table.Schema.SchemaName}Api"},
                StoredProcedureName = $"{table.TableName}AddUpdate",
                ParameterSetName = table.TableName,
                ResultSetName = $"{table.TableName}AddUpdateResult",
                ReturnsMultipleRows = true,
                DataOperationIs = COperationIs.Add | COperationIs.Update | COperationIs.CRUD
            };
            table.InsertStoredProcedure = storedProcedure;

            storedProcedure.Parameter.AddRange(GetParameters(table));
            var primaryKeyCol = table.Column.FirstOrDefault(c => c.IsPrimaryKey);
            var insertUpdateColumnList = GetInsertUpdateColumnList(table);
            var insertUpdateColumnMappings = GetInsertUpdateColumnMappings(table);
            var insertUpdateColumnValues = GetInsertUpateColumnValues(table);
            var redefines = GetRedefines(table);
            var pkCompares = GetPrimaryKeyCompares(table);
            var rowVersionColumn = table.GetRowVersionColumn();
            var insertCondition = rowVersionColumn != null
                ? $" AND NEW_T.[{primaryKeyCol.ColumnName}] = 0  AND NEW_T.[{rowVersionColumn.ColumnName}] IS NULL "
                : string.Empty;
            var updateCondition = rowVersionColumn != null
                ? $" AND (NEW_T.[{rowVersionColumn.ColumnName}] IS NULL OR T.[{rowVersionColumn.ColumnName}] = NEW_T.[{rowVersionColumn.ColumnName}]) "
                : string.Empty;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("SET NOCOUNT ON;");
            stringBuilder.AppendLine(
                "SET XACT_ABORT ON;"); //http://michaeljswart.com/2015/10/dont-abandon-your-transactions/
            stringBuilder.AppendLine();
            //create table variable to hold output
            stringBuilder.Append($@"DECLARE @Output TABLE (Action VARCHAR(20),
                                        [{primaryKeyCol.ColumnName}] {
                    SqlMapper.DbTypeToSqlDataTypeOption(primaryKeyCol.ColumnType)
                }");
            if (primaryKeyCol.ColumnLength > 0)
                stringBuilder.Append($"({primaryKeyCol.ColumnLength})");

            var rowVersionCol = string.Empty;
            var insertedRowVersionCol = string.Empty;

            if (rowVersionColumn != null)
            {
                rowVersionCol = $", {rowVersionColumn.ColumnName}";
                insertedRowVersionCol =
                    $", inserted.[{rowVersionColumn.ColumnName}] as '[{rowVersionColumn.ColumnName}]'";
                stringBuilder.Append($", [{rowVersionColumn.ColumnName}]  binary(8)");
            }
            stringBuilder.AppendLine($");");


            stringBuilder.AppendLine();
            stringBuilder.AppendLine(
                $@"MERGE [{table.Schema.SchemaName}].[{table.TableName}] WITH (HOLDLOCK) AS T
                    USING (SELECT {redefines} ) as NEW_T
                    ON {pkCompares}
                    WHEN MATCHED {updateCondition} THEN
                     UPDATE
                     SET 
                        {insertUpdateColumnMappings}
                     WHEN NOT MATCHED {insertCondition} THEN

                     INSERT
                     (
                         {insertUpdateColumnList}
                     )
                     VALUES
                     (
                        {insertUpdateColumnValues}
                     )
                    OUTPUT	
                        $action, inserted.[{primaryKeyCol.ColumnName}] as '[{primaryKeyCol.ColumnName}]' {
                        insertedRowVersionCol
                    } INTO @Output;

                    --TODO: Use OUTPUT to insert original values into Audit tables

                    SELECT Action, [{primaryKeyCol.ColumnName}] {rowVersionCol} FROM @Output;");

            /*
             var identityCol = table.Column.FirstOrDefault(c => c.IsIdentity);
             if (identityCol != null)
             {
                 stringBuilder.AppendLine();
                 stringBuilder.AppendLine($"SELECT CAST(SCOPE_IDENTITY() as {SqlMapper.DbTypeToSqlDbType( identityCol.ColumnType)})");

             }
             */
            storedProcedure.StoredProcedureBody = stringBuilder.ToString();

            storedProcedure.ResultSet.Add(new CColumn(storedProcedure)
            {
                ColumnName = "Action",
                ColumnTypeRaw = "varchar",
                ColumnSqlDbType = SqlDbType.VarChar,
                ColumnType = DbType.AnsiString,
                ColumnLength = 20
            });
            storedProcedure.ResultSet.Add(new CColumn(storedProcedure)
            {
                ColumnName = primaryKeyCol.ColumnName,
                ColumnTypeRaw = primaryKeyCol.ColumnTypeRaw,
                ColumnSqlDbType = primaryKeyCol.ColumnSqlDbType,
                ColumnType = primaryKeyCol.ColumnType,
                ColumnLength = primaryKeyCol.ColumnLength
            });
            if (rowVersionColumn != null)
                storedProcedure.ResultSet.Add(new CColumn(storedProcedure)
                {
                    ColumnName = rowVersionColumn.ColumnName,
                    ColumnTypeRaw = rowVersionColumn.ColumnTypeRaw,
                    ColumnType = rowVersionColumn.ColumnType,

                    ColumnSqlDbType = rowVersionColumn.ColumnSqlDbType,
                    ColumnLength = rowVersionColumn.ColumnLength
                });
            return storedProcedure;
        }

        private object GetRedefines(CTable table)
        {
            var pkColumns = table.GetPrimaryKeyColumns();
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var column in table.Column)
            {
                if (column.IsModifiedDate)
                    continue;
                if (column.IsCreatedDate)
                    continue;

                if (!first)
                    stringBuilder.Append(", ");
                stringBuilder.Append($" @{column.ColumnName} AS [{column.ColumnName}] ");
                first = false;
            }

            return stringBuilder.ToString();
        }

        private string GetPrimaryKeyCompares(CTable table)
        {
            var pkColumns = table.GetPrimaryKeyColumns();
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var pkColumn in pkColumns)
            {
                if (!first)
                    stringBuilder.Append(" AND ");
                stringBuilder.Append($"T.[{pkColumn.ColumnName}] = NEW_T.[{pkColumn.ColumnName}]");
                first = false;
            }

            return stringBuilder.ToString();
        }

        private object GetInsertUpdateColumnMappings(CTable table)
        {
            var pkColumns = table.GetPrimaryKeyColumns();
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var column in table.Column)
            {
                if (column.IsIdentity)
                    continue;
                if (column.IsComputed)
                    continue;
                if (column.IsRowVersion)
                    continue;
                if (column.IsCreatedDate)
                    continue;

                if (pkColumns.Contains(column))
                    continue;

                if (!first)
                    stringBuilder.Append(", ");

                if (column.IsModifiedDate)
                    stringBuilder.Append($"T.[{column.ColumnName}] = GetUtcDate()");
                else
                    stringBuilder.Append($"T.[{column.ColumnName}] = NEW_T.[{column.ColumnName}]");

                first = false;
            }
            return stringBuilder.ToString();
        }

        private string GetInsertUpateColumnValues(CTable table)
        {
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var column in table.Column)
            {
                if (column.IsIdentity)
                    continue;
                if (column.IsComputed)
                    continue;
                if (column.IsRowVersion)
                    continue;

                if (!first)
                    stringBuilder.Append(", ");
                first = false;
                if (column.IsCreatedDate)
                    stringBuilder.Append($"GetUtcDate()");
                else if (column.IsModifiedDate)
                    stringBuilder.Append($"NULL");
                else
                    stringBuilder.Append($"NEW_T.[{column.ColumnName}]");
            }

            return stringBuilder.ToString();
        }

        private string GetInsertUpdateColumnList(CTable table)
        {
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var column in table.Column)
            {
                if (column.IsIdentity)
                    continue;
                if (column.IsComputed)
                    continue;

                if (column.IsRowVersion)
                    continue;
                if (!first)
                    stringBuilder.Append(", ");

                stringBuilder.Append($"[{column.ColumnName}]");
                first = false;
            }
            return stringBuilder.ToString();
        }

        private List<CStoredProcedureParameter> GetParameters(CTable table)
        {
            var parameters = new List<CStoredProcedureParameter>();
            foreach (var column in table.Column)
            {
                if (column.IsCreatedDate)
                    continue;
                if (column.IsModifiedDate)
                    continue;

                var parameter = new CStoredProcedureParameter
                {
                    ParameterName = $"{column.ColumnName}",
                    ParameterType = column.ColumnType,
                    ParameterTypeRaw = column.ColumnTypeRaw,
                    ParameterLength = column.ColumnLength,
                    DefaultToNull = column.IsRowVersion,
                    SourceColumn = column
                };
                parameters.Add(parameter);
            }

            return parameters;
        }

        #endregion Methods
    }
}