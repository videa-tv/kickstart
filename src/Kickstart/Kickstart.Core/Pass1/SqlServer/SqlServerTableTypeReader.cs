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
    public interface ISqlServerTableTypeReader
    {
        string ConnectionString { get; set; }
        CTableType Read(string schema, string tableTypeName, string tableTypeText);
    }

    public class SqlServerTableTypeReader : IReader, ISqlServerTableTypeReader
    {
        #region Properties

        public string ConnectionString { get; set; }

        #endregion Properties

        #region Fields

        #endregion Fields

        #region Constructors

        #endregion Constructors

        #region Methods

        public CTableType Read(string schema, string tableTypeName, string tableTypeText)
        {
            var tableType = new CTableType(DataStoreTypes.SqlServer);
            tableType.Schema = new CSchema {SchemaName = schema};
            tableType.TableName = tableTypeName;
            tableType.TableTypeBody = GetTableTypeBody(tableTypeText);

            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                var sqlViewQuery =
                    $@"select   
                    c.name AS column_name,
                    st.name AS data_type,
                    c.is_nullable,  
                    c.max_length as character_maximum_length
                    ,s.name
                    from sys.table_types tt
                    inner join sys.columns c on c.object_id = tt.type_table_object_id
                    --INNER JOIN sys.systypes AS ST  ON ST.xtype = c.system_type_id
                    INNER JOIN sys.systypes AS ST  ON ST.xusertype = c.user_type_id
			
				join sys.schemas s on s.schema_id = tt.schema_id
                    WHERE tt.name= '{tableTypeName}' and s.name = '{tableType.Schema.SchemaName}'";

                sqlConnection.Open();
                var sqlCommand = new SqlCommand(sqlViewQuery, sqlConnection);
                var dataReader = sqlCommand.ExecuteReader();

                var dataTable = new DataTable();

                dataTable.Columns.Add(new DataColumn {ColumnName = "column_name"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "data_type"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "is_nullable"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "character_maximum_length", DataType = typeof(int)});
                dataTable.Columns.Add(new DataColumn {ColumnName = "tabletype_name"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "tabletype_schema"});

                while (dataReader.Read())
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["column_name"] = dataReader["column_name"];
                    dataRow["data_type"] = dataReader["data_type"];
                    dataRow["is_nullable"] = dataReader["is_nullable"];
                    dataRow["character_maximum_length"] = dataReader["character_maximum_length"];
                    dataRow["tabletype_name"] = tableTypeName;
                    dataRow["tabletype_schema"] = schema;

                    dataTable.Rows.Add(dataRow);
                }

                foreach (var row2 in dataTable.Rows)
                {
                    var row = row2 as DataRow;
                    var column = new CColumn(tableType)
                    {
                        ColumnName = (string) row[0],
                        ColumnTypeRaw = (string) row[1],
                        ColumnSqlDbType = SqlMapper.ParseValueAsSqlDbType((string) row[1]),
                        ColumnType = SqlMapper.GetDbType((string) row[1]),
                        ColumnLength = row[3] != DBNull.Value ? (int) row[3] : -1
                    };

                    tableType.Column.Add(column);
                }
                return tableType;
            }
        }

        private string GetTableTypeBody(string sqlText)
        {
            var createTableTypeBody = new CodeWriter();

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
                var createTypeTableStatement = statement as CreateTypeTableStatement;

                if (createTypeTableStatement == null)
                    continue;

                string scriptOut;
                scriptGen.GenerateScript(createTypeTableStatement, out scriptOut);

                createTableTypeBody.WriteLine(scriptOut);
            }

            return createTableTypeBody.ToString();
        }

        #endregion Methods
    }
}