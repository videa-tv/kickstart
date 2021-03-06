using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Kickstart.Interface;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass1.SqlServer
{
    public interface ISqlServerViewReader
    { 
        DataTable Read(string connectionString, string sqlView, string viewText);
        Dictionary<string, string> ParseColumnDescriptions(string sqlViewText);
    }

    public class SqlServerViewReader : IReader, ISqlServerViewReader
    {
        #region Properties

      
        #endregion Properties

        #region Fields

        #endregion Fields

        #region Constructors

        #endregion Constructors

        #region Methods

        public DataTable Read(string connectionString, string sqlView, string viewText)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                var sqlViewQuery =
                    $@"select column_name, data_type, is_nullable, 
                          character_maximum_length, table_name, table_schema
                from
                    information_schema.columns
                where 
                    TABLE_NAME = '{sqlView}'";

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
                dataTable.Columns.Add(new DataColumn {ColumnName = "column_description"});

                var columns = ParseColumnDescriptions(viewText);
                ;
                while (dataReader.Read())
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["column_name"] = dataReader["column_name"];
                    dataRow["data_type"] = dataReader["data_type"];
                    dataRow["is_nullable"] = dataReader["is_nullable"];
                    dataRow["character_maximum_length"] = dataReader["character_maximum_length"];
                    dataRow["table_name"] = dataReader["table_name"];
                    dataRow["table_schema"] = dataReader["table_schema"];
                    /*
                    if (columns == null)
                    {
                        
                      
                        var viewText = GetViewText((string)dataRow["table_schema"], (string)dataRow["table_name"]);
                        
                        columns = ParseColumnDescriptions(viewText);
                    }*/
                    if (columns.ContainsKey((string) dataRow["column_name"]))
                        dataRow["column_description"] = columns[(string) dataRow["column_name"]];
                    dataTable.Rows.Add(dataRow);
                }
                return dataTable;
            }
        }

        private string GetViewText(string connectionString, string schema, string viewName)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                var sqlViewTextQuery = $@"select definition
                            from sys.objects     o
                            join sys.sql_modules m on m.object_id = o.object_id
                            where o.object_id = object_id( '[{schema}].[{viewName}]')
                              and o.type      = 'V'";
                sqlConnection.Open();
                var sqlCommand2 = new SqlCommand(sqlViewTextQuery, sqlConnection);
                var dataReader2 = sqlCommand2.ExecuteReader();
                dataReader2.Read();
                return dataReader2.GetString(0);
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

                CreateViewStatement createViewStatement = null;
                foreach (var batch in sqlScript.Batches)
                {
                    var cts = batch.Statements.FirstOrDefault(s => s is CreateViewStatement);
                    if (cts != null)
                        createViewStatement = cts as CreateViewStatement;
                }
                if (createViewStatement == null)
                    continue;
                if (createViewStatement.SchemaObjectName.SchemaIdentifier == null)
                    continue;

                var schema = createViewStatement.SchemaObjectName.SchemaIdentifier.Value;
                var viewName = createViewStatement.SchemaObjectName.BaseIdentifier.Value;
                foreach (var fg in sqlFragment.ScriptTokenStream)
                    //find the comment, using parser
                    if (fg.TokenType == TSqlTokenType.SingleLineComment && fg.Line == fileLineNumber)
                    {
                        //find the column name
                        var columnFragment =
                            sqlFragment.ScriptTokenStream.Reverse().FirstOrDefault(
                                s => s.Line == fg.Line &&
                                     (s.TokenType == TSqlTokenType.QuotedIdentifier ||
                                      s.TokenType == TSqlTokenType.Identifier ||
                                      s.TokenType == TSqlTokenType.AsciiStringLiteral));

                        if (columnFragment != null && !string.IsNullOrEmpty(columnFragment.Text))
                        {
                            var columnName = columnFragment.Text;
                            columns.Add(columnName, docSnippet);
                            // AddExtendedPropertyToScript(schema, tableName, columnName, docSnippet);
                        }
                    }
            } while (fileLine != null);

            return columns;
        }

        #endregion Methods
    }
}