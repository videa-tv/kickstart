using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using Kickstart.Interface;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass1.SqlServer
{
    public interface ISqlServerTableReader
    {
        string ConnectionString { get; set; }
        DataTable Read(string schema, string sqlTable);
        IEnumerable<TableForeignKeysDto> ReadForeignKeys(string schema, string sqlTable);
        Dictionary<string, string> ParseColumnDescriptions(string sqlViewText);
    }

    public class SqlServerTableReader : IReader, ISqlServerTableReader
    {
        #region Properties

        public string ConnectionString { get; set; }

        #endregion Properties

        #region Fields

        #endregion Fields

        #region Constructors

        #endregion Constructors

        #region Methods

        public DataTable Read(string schema, string sqlTable)
        {
            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                var sqlViewQuery =
                    $@"
SELECT DISTINCT C.column_name,
       C.data_type,
       C.is_nullable,
       C.character_maximum_length,
       C.table_name,
       C.table_schema,
       CONVERT(BIT,
               CASE
                   WHEN pk.COLUMN_NAME IS NOT NULL
                   THEN 1
                   ELSE 0
               END) AS IsPrimaryKey,
       CONVERT(BIT, COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA+'.'+c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity')) AS IsIdentity,
        CONVERT(BIT, COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA+'.'+c.TABLE_NAME), c.COLUMN_NAME, 'IsComputed')) AS IsComputed,
         c.ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS c
     LEFT JOIN
(
    SELECT ku.TABLE_CATALOG,
           ku.TABLE_SCHEMA,
           ku.TABLE_NAME,
           ku.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
         INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                                                                 AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
) pk ON c.TABLE_CATALOG = pk.TABLE_CATALOG
        AND c.TABLE_SCHEMA = pk.TABLE_SCHEMA
        AND c.TABLE_NAME = pk.TABLE_NAME
        AND c.COLUMN_NAME = pk.COLUMN_NAME
 WHERE 
        c.TABLE_SCHEMA ='{schema}' AND
                    c.TABLE_NAME = '{sqlTable}'
ORDER BY c.TABLE_SCHEMA,
         c.TABLE_NAME,
         c.ORDINAL_POSITION
               ";

                sqlConnection.Open();
                var sqlCommand = new SqlCommand(sqlViewQuery, sqlConnection);
                var dataReader = sqlCommand.ExecuteReader();

                var dataTable = new DataTable();

                dataTable.Columns.Add(new DataColumn {ColumnName = "column_name"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "data_type"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "is_nullable"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "character_maximum_length", DataType = typeof(int)});
                dataTable.Columns.Add(new DataColumn {ColumnName = "table_name"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "table_schema"});
                dataTable.Columns.Add(new DataColumn {ColumnName = "IsPrimaryKey", DataType = typeof(bool)});
                dataTable.Columns.Add(new DataColumn {ColumnName = "IsIdentity", DataType = typeof(bool)});
                dataTable.Columns.Add(new DataColumn {ColumnName = "IsComputed", DataType = typeof(bool)});
                while (dataReader.Read())
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["column_name"] = dataReader["column_name"];
                    dataRow["data_type"] = dataReader["data_type"];
                    dataRow["is_nullable"] = dataReader["is_nullable"];
                    dataRow["character_maximum_length"] = dataReader["character_maximum_length"];
                    dataRow["table_name"] = dataReader["table_name"];
                    dataRow["table_schema"] = dataReader["table_schema"];
                    dataRow["IsPrimaryKey"] = dataReader["IsPrimaryKey"];
                    dataRow["IsIdentity"] = dataReader["IsIdentity"];
                    dataRow["IsComputed"] = dataReader["IsComputed"];
                    dataTable.Rows.Add(dataRow);
                }
                return dataTable;
            }
        }

        public Dictionary<string, string> ParseColumnDescriptions(string sqlViewText)
        {
            var columns = new Dictionary<string, string>();

            var file = new StringReader(sqlViewText);

            var fileLine = string.Empty;
            var fileLineNumber = 0;
            do
            {
                fileLineNumber++;
                fileLine = file.ReadLine();
                if (fileLine == null)
                    continue;

                var startTag = "<d>";
                var startOfDocTag = fileLine.IndexOf(startTag, StringComparison.CurrentCultureIgnoreCase);
                var endOfDocTag = fileLine.IndexOf("</d>", StringComparison.CurrentCultureIgnoreCase);
                if (startOfDocTag < 0)
                    continue;
                if (endOfDocTag < 0)
                    continue;

                var docSnippet = fileLine.Substring(startOfDocTag + startTag.Length,
                    endOfDocTag - (startOfDocTag + startTag.Length));

                var parser = new TSql120Parser(true);

                TextReader txtRdr = new StringReader(sqlViewText);
                IList<ParseError> errors;
                var sqlFragment = parser.Parse(txtRdr, out errors);
                var sqlScript = sqlFragment as TSqlScript;

                CreateTableStatement createTableStatement = null;
                foreach (var batch in sqlScript.Batches)
                {
                    var cts = batch.Statements.FirstOrDefault(s => s is CreateTableStatement);
                    if (cts != null)
                        createTableStatement = cts as CreateTableStatement;
                }
                if (createTableStatement == null)
                    continue;
                if (createTableStatement.SchemaObjectName.SchemaIdentifier == null)
                    continue;

                var schema = createTableStatement.SchemaObjectName.SchemaIdentifier.Value;
                var tableName = createTableStatement.SchemaObjectName.BaseIdentifier.Value;
                foreach (var fg in sqlFragment.ScriptTokenStream)
                    //find the comment, using parser
                    if (fg.TokenType == TSqlTokenType.SingleLineComment && fg.Line == fileLineNumber)
                    {
                        var columnFragment =
                            sqlFragment.ScriptTokenStream.FirstOrDefault(
                                s => s.Line == fg.Line && s.TokenType == TSqlTokenType.QuotedIdentifier);

                        if (columnFragment != null)
                        {
                            var columnName = columnFragment.Text;
                            columns.Add(columnName, docSnippet);
                            // AddExtendedPropertyToScript(schema, tableName, columnName, docSnippet);
                        }
                    }
            } while (fileLine != null);

            return columns;
        }

        public IEnumerable<TableForeignKeysDto> ReadForeignKeys(string schema, string sqlTable)
        {
            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                var sqlQuery =
                    $@"SELECT  
                         KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME 
	                     ,KCU1.TABLE_SCHEMA as FK_TABLE_SCHEMA
                        ,KCU1.TABLE_NAME AS FK_TABLE_NAME 
                        ,KCU1.COLUMN_NAME AS FkColumnName 
                        ,KCU1.ORDINAL_POSITION AS FK_ORDINAL_POSITION 
                        ,KCU2.CONSTRAINT_NAME AS REFERENCED_CONSTRAINT_NAME 
	                    ,KCU2.TABLE_SCHEMA as ReferencedTableSchema
                        ,KCU2.TABLE_NAME AS ReferencedTableName 
                        ,KCU2.COLUMN_NAME AS ReferencedColumnName 
                        ,KCU2.ORDINAL_POSITION AS REFERENCED_ORDINAL_POSITION 
                    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC 

                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU1 
                        ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG  
                        AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA 
                        AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME 

                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU2 
                        ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG  
                        AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA 
                        AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME 
                        AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION 
	                    WHERE 
		                    KCU1.TABLE_SCHEMA ='{schema}'
	                    AND	KCU1.TABLE_NAME = '{sqlTable}'";

                var results =sqlConnection.Query< TableForeignKeysDto>(sqlQuery);

                int x = 1;

                return results;
            }

            
        }

        #endregion Methods
    }

    public class TableForeignKeysDto
    {

        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ReferencedTableSchema { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
    }
}