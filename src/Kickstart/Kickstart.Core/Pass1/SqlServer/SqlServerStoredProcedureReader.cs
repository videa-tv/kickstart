using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass1.SqlServer
{
    public interface ISqlServerStoredProcedureReader : IReader
    {
        CStoredProcedure Read(string connectionString, string schema, string storedProcedureName, string storedProcedureText);
    }

    public class SqlServerStoredProcedureReader :  ISqlServerStoredProcedureReader
    {
        #region Properties

        public string ConnectionString { get; set; }

        #endregion Properties

        #region Fields

        #endregion Fields

        #region Constructors

      
        #endregion Constructors

        #region Methods

        public CStoredProcedure Read(string connectionString, string schema, string storedProcedureName, string storedProcedureText)
        {
            this.ConnectionString = connectionString;
            var storedProcedure = new CStoredProcedure(DataStoreTypes.SqlServer);
            storedProcedure.Schema = new CSchema {SchemaName = schema};
            storedProcedure.StoredProcedureName = storedProcedureName;
            storedProcedure.StoredProcedureBody = GetStoredProcBody(storedProcedureText);

            storedProcedure.Parameter.AddRange(GetParameters(ConnectionString, storedProcedureText, storedProcedure));

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                var sqlViewQuery =
                    $@"DECLARE @sql NVARCHAR(MAX) = N'EXEC  [{schema}].[{storedProcedureName}];';
                    SELECT dm.name as 'column_name', t.name as 'data_type',dm.is_nullable,dm.max_length as 'character_maximum_length',is_hidden
    FROM sys.dm_exec_describe_first_result_set(@sql, NULL, 1) dm
	join sys.types t on dm.system_type_id = t.system_type_id
    and dm.system_type_id = t.user_type_id
    WHERE is_hidden=0;";

                sqlConnection.Open();
                var sqlCommand = new SqlCommand(sqlViewQuery, sqlConnection);
                var dataReader = sqlCommand.ExecuteReader();


                var dataTable = new DataTable();

                dataTable.Columns.Add(new DataColumn {ColumnName = "column_name"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "data_type"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "is_nullable"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "character_maximum_length", DataType = typeof(int)});
                dataTable.Columns.Add(new DataColumn {ColumnName = "storedprocedure_name"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "storedprocedure_schema"});

                while (dataReader.Read())
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["column_name"] = dataReader["column_name"];
                    dataRow["data_type"] = dataReader["data_type"];
                    dataRow["is_nullable"] = dataReader["is_nullable"];
                    dataRow["character_maximum_length"] = dataReader["character_maximum_length"];
                    dataRow["storedprocedure_name"] = storedProcedureName;
                    dataRow["storedprocedure_schema"] = schema;

                    dataTable.Rows.Add(dataRow);
                }

                foreach (var row2 in dataTable.Rows)
                {
                    var row = row2 as DataRow;
                    var column = new CColumn(storedProcedure)
                    {
                        ColumnName =row[0] != DBNull.Value ? (string) row[0] : null,
                        ColumnTypeRaw = (string) row[1],
                        ColumnSqlDbType = SqlMapper.ParseValueAsSqlDbType((string) row[1]),
                        ColumnType = SqlMapper.GetDbType((string) row[1]),
                        ColumnLength = row[3] != DBNull.Value ? (int) row[3] : -1
                    };

                    storedProcedure.ResultSet.Add(column);
                }
                if (storedProcedure.HasResultSet)
                {
                    storedProcedure.ResultSetName = storedProcedure.StoredProcedureName + "Dto"; // "ResultSet";
                }
                return storedProcedure;
            }
        }

        private string GetStoredProcBody(string sqlText)
        {
            var storedProcBody = new CodeWriter();

            var parser = new TSql120Parser(false);

            var statementList = new StatementList();
            IList<ParseError> errors;
            var script2 = parser.Parse(new StringReader(sqlText), out errors) as TSqlScript;
            if (errors.Count > 0)
            {
                var errorList = new StringBuilder();
                foreach (var error in errors)
                    errorList.AppendLine($"{error.Message}<br/>");
                throw new ApplicationException(errorList.ToString());
            }

            var scriptGen = new Sql120ScriptGenerator();


            foreach (var batch2 in script2.Batches)
            foreach (var statement in batch2.Statements)
            {
                var createProcedureStatement = statement as CreateProcedureStatement;

                if (createProcedureStatement == null)
                    continue;
                if (createProcedureStatement.StatementList == null)
                    continue;

                foreach (var statement2 in createProcedureStatement.StatementList.Statements)
                {
                    string scriptOut;
                    scriptGen.GenerateScript(statement2, out scriptOut);

                        if (statement2 is MergeStatement && !scriptOut.EndsWith(";"))
                            scriptOut += ";";

                    storedProcBody.WriteLine(scriptOut);
                }
            }

            return storedProcBody.ToString();
        }

        private List<CStoredProcedureParameter> GetParameters(string connectionString, string sqlText,
            CStoredProcedure storedProcedure)
        {
            var storedProcedureParameters = new List<CStoredProcedureParameter>();

            var scriptGen = new Sql120ScriptGenerator();

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var parser = new TSql120Parser(false);

                var statementList = new StatementList();
                IList<ParseError> errors;
                var script2 = parser.Parse(new StringReader(sqlText), out errors) as TSqlScript;
                if (errors.Count > 0)
                {
                    var errorList = new StringBuilder();
                    foreach (var error in errors)
                        errorList.AppendLine($"{error.Message}<br/>");
                    throw new ApplicationException(errorList.ToString());
                }
                foreach (var batch2 in script2.Batches)
                foreach (var statement in batch2.Statements)
                {
                    var createProcedureStatement = statement as CreateProcedureStatement;

                    if (createProcedureStatement == null)
                        continue;

                    foreach (var param in createProcedureStatement.Parameters)
                    {
                        //(new System.Collections.Generic.Mscorlib_CollectionDebugView<Microsoft.SqlServer.TransactSql.ScriptDom.Literal>
                        //  (((Microsoft.SqlServer.TransactSql.ScriptDom.ParameterizedDataTypeReference)param.DataType).Parameters).Items[0]).Value;
                        var length = 0;
                        if ((param.DataType as ParameterizedDataTypeReference).Parameters.Count > 0)
                        {
                            var lengthString = (param.DataType as ParameterizedDataTypeReference).Parameters[0].Value;
                            if ((param.DataType as ParameterizedDataTypeReference).Parameters[0] is Microsoft.SqlServer.TransactSql.ScriptDom.MaxLiteral)
                            {
                                    length = -1;
                            }
                            else
                            {

                                length = int.Parse(lengthString);
                            }
                        }
                        var storedProcedureParameter = new CStoredProcedureParameter
                        {
                            ParameterName = param.VariableName.Value.Replace("@", "").Replace("_Collection",""),

                            ParameterTypeIsUserDefined = param.DataType is UserDataTypeReference,
                            ParameterTypeRaw = param.DataType.Name.BaseIdentifier.Value,
                            ParameterTypeRawSchema = param.DataType.Name?.SchemaIdentifier?.Value,
                            SourceColumn =
                                new CColumn(storedProcedure) {ColumnName = param.VariableName.Value.Replace("@", "")},
                            IsCollection = param.VariableName.Value.EndsWith("_Collection")
                        };
                        if (length > 0)
                        {
                            storedProcedureParameter.ParameterLength = length;
                            storedProcedureParameter.SourceColumn.ColumnLength = length;
                        }
                        if (!storedProcedureParameter.ParameterTypeIsUserDefined)
                            storedProcedureParameter.ParameterType =
                                SqlMapper.SqlDbTypeToDbType(
                                    SqlMapper.ParseValueAsSqlDbType(param.DataType.Name.BaseIdentifier.Value));
                        storedProcedureParameters.Add(storedProcedureParameter);
                    }
                }
                sqlConnection.Close();
            }
            return storedProcedureParameters;
        }

        #endregion Methods
    }
}