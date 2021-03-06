using System.Collections.Generic;
using System.Text;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.SqlServer
{
    public interface ICTableToSStoredProcedureDeleteConverter
    {
        CStoredProcedure Convert(CTable table);
    }

    public class CTableToSStoredProcedureDeleteConverter : ICTableToSStoredProcedureDeleteConverter
    {
        #region Methods

        public CStoredProcedure Convert(CTable table)
        {
            var storedProcedure = new CStoredProcedure( DataStoreTypes.SqlServer) {DataOperationIs = COperationIs.Delete | COperationIs.CRUD};
            storedProcedure.Schema = new CSchema {SchemaName = $"{table.Schema.SchemaName}Api"};
            storedProcedure.StoredProcedureName = $"{table.TableName}Delete";
            //storedProcedure.ResultSetName = table.TableName;
            storedProcedure.Parameter = new List<CStoredProcedureParameter>();

            var pkColumns = table.GetPrimaryKeyColumns();
            foreach (var pkColumn in pkColumns)
            {
                var parameter = new CStoredProcedureParameter
                {
                    ParameterName = $"{pkColumn.ColumnName}",
                    ParameterType = pkColumn.ColumnType,

                    ParameterTypeRaw = pkColumn.ColumnTypeRaw,
                    ParameterLength = pkColumn.ColumnLength,
                    SourceColumn = pkColumn
                };
                storedProcedure.Parameter.Add(parameter);
            }
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"DELETE FROM [{table.Schema.SchemaName}].[{table.TableName}] ");
            stringBuilder.AppendLine("WHERE");
            var first = true;
            foreach (var pkColumn in pkColumns)
            {
                if (!first)
                    stringBuilder.Append(" AND ");
                stringBuilder.AppendLine($"\t\t[{pkColumn.ColumnName}] = @{pkColumn.ColumnName}");
                first = false;
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