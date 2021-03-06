using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.SqlServer
{
    public class CTableToMySqlTableConverter : ICTableToTableConverter
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

            //generate DDL

            var codeWriter = new CodeWriter();
            codeWriter.WriteLine($@"CREATE TABLE IF NOT EXISTS {table.Schema.SchemaName.WrapReservedAndSnakeCase( DataStoreTypes.MySql, table.ConvertToSnakeCase)}.{table.TableName.WrapReservedAndSnakeCase(DataStoreTypes.MySql, table.ConvertToSnakeCase)} (");
            codeWriter.Indent();

            var first = true;
            foreach (var col in table.Column)
            {
                if (!first)
                    codeWriter.WriteLine(",");
                first = false;

                codeWriter.Write($"{col.ColumnName.WrapReservedAndSnakeCase( DataStoreTypes.MySql, table.ConvertToSnakeCase)}");
                codeWriter.Write("\t");
                

                
                codeWriter.Write($" {SqlMapper.MySqlDbTypeToMySql(SqlMapper.DbTypeToMySqlDbType(col.ColumnType))}");
                if (col.DoesNeedLength())
                {
                    codeWriter.Write($"({col.ColumnLength})");
                }
                codeWriter.Write("\t");

                if (col.IsIdentity)
                {
                    codeWriter.Write($" AUTO_INCREMENT");
                }
                
                if (col.IsPrimaryKey)
                {
                    codeWriter.Write($" PRIMARY KEY");
                }

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
            codeWriter.WriteLine();
            codeWriter.WriteLine(")  ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;");
            return codeWriter.ToString();
            ;
        }
        
        #endregion Methods
    }
}