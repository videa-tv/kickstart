using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Npgsql;
using NpgsqlTypes;
using Npgsql.PostgresTypes;
namespace Kickstart.Pass2.SqlServer
{
    public class CTableToPostgresTableConverter : ICTableToTableConverter
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
            //var converter = new CTableToCreateTableStatementConverter();
            //var createTable = converter.Convert(table);

            //generate DDL
            var schemaName = table.Schema.SchemaName.WrapReservedAndSnakeCase(table.DatabaseType, table.ConvertToSnakeCase);

            var codeWriter = new CodeWriter();
            codeWriter.WriteLine($@"CREATE TABLE IF NOT EXISTS {schemaName}.{table.TableName.WrapReservedAndSnakeCase(table.DatabaseType, table.ConvertToSnakeCase)} (");
            codeWriter.Indent();

            var first = true;
            foreach (var col in table.Column)
            {
                if (!first)
                    codeWriter.WriteLine(",");
                first = false;

                codeWriter.Write($"{col.ColumnName.WrapReservedAndSnakeCase(table.DatabaseType, table.ConvertToSnakeCase)}");
                
                
                if (col.IsIdentity)
                {
                    if (col.ColumnSqlDbType == SqlDbType.BigInt)
                    {
                        codeWriter.Write($" bigserial");
                    }
                    else
                        codeWriter.Write($" serial");

                }
                else
                {
                    codeWriter.Write($" {SqlMapper.NpgsqlDbTypeToPostgres(SqlMapper.DbTypeToNpgsqlDbType(col.ColumnType))}");
                    if (col.DoesNeedLength())
                    {
                        codeWriter.Write($"({col.ColumnLength})");
                    }
                }
                if (!col.IsNullable)
                {
                    codeWriter.Write(" NOT NULL");
                }
                

            }
            var primaryKeys = table.Column.Where(c => c.IsPrimaryKey).ToList();
            if (primaryKeys.Count > 0)
            {
                codeWriter.WriteLine(",");
                codeWriter.Write($"CONSTRAINT {table.TableName}_pkey PRIMARY KEY (");

                var firstPkey = true;
                foreach (var pk in primaryKeys)
                {
                    if (!firstPkey)
                    {
                        codeWriter.Write(", ");
                    }
                    firstPkey = false;
                    codeWriter.Write($"{pk.ColumnName.WrapReservedAndSnakeCase(table.DatabaseType, table.ConvertToSnakeCase)}");

                }
                codeWriter.WriteLine(")");
                
            }
            
            codeWriter.Unindent();
            /*
            foreach (var column in table.Column)
                if (!string.IsNullOrEmpty(column.ColumnDescription))
                {
                    codeWriter.WriteLine();
                    codeWriter.WriteLine("GO");
                    codeWriter.WriteLine();
                    codeWriter.WriteLine(GetExtendedPropertyScript(table.Schema.SchemaName, table.TableName,
                        column.ColumnName, column.ColumnDescription));
                }*/

            codeWriter.WriteLine(");");
            return codeWriter.ToString();
            ;
        }

      

        #endregion Methods
    }
}