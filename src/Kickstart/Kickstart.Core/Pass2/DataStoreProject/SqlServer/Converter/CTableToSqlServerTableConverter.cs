using System;
using System.Collections.Generic;
using System.IO;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass2.SqlServer
{
    public class CTableToSqlServerTableConverter : ICTableToTableConverter
    {
        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors

        #region Methods

        public string Convert(CTable table)
        {

            var converter = new CTableToCreateTableStatementConverter();
            var createTable = converter.Convert(table);


            string tableScript;
            var scriptGen = new Sql120ScriptGenerator();

            scriptGen.GenerateScript(createTable, out tableScript);
            /*
            //generate DDL
            var script = new TSqlScript();
            var batch = new TSqlBatch();
            script.Batches.Add(batch);
            batch.Statements.Add(createTable);
            var dacpacModel = new TSqlModel(SqlServerVersion.Sql120, new TSqlModelOptions());
            dacpacModel.AddObjects(script);

            string[] parts = { table.Schema.SchemaName, table.TableName };

            var existing = dacpacModel.GetObject(Table.TypeClass, new ObjectIdentifier(parts), DacQueryScopes.All);
            var tableScript = existing.GetScript();
            */
            var codeWriter = new CodeWriter();
            codeWriter.WriteLine(tableScript);

            foreach (var column in table.Column)
                if (!string.IsNullOrEmpty(column.ColumnDescription))
                {
                    codeWriter.WriteLine();
                    codeWriter.WriteLine("GO");
                    codeWriter.WriteLine();
                    codeWriter.WriteLine(GetExtendedPropertyScript(table.Schema.SchemaName, table.TableName,
                        column.ColumnName, column.ColumnDescription));
                }
            return codeWriter.ToString();
            ;
        }

        private string GetExtendedPropertyScript(string schema, string tableName, string columnName, string docSnippet)
        {
            var sql =
                $@"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{
                        docSnippet
                    }' , @level0type=N'SCHEMA',@level0name=N'{schema}', @level1type=N'TABLE',@level1name=N'{
                        tableName
                    }', @level2type=N'COLUMN',@level2name=N'{columnName}'";
            sql += Environment.NewLine;
            sql += "GO";
            sql += Environment.NewLine;
            return sql;
            //System.IO.File.AppendAllText(_options.OutputFile, sql);
        }

        #endregion Methods
    }
}