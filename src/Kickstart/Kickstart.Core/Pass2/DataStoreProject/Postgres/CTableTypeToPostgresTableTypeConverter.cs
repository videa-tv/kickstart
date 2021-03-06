using System.Data;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass2.SqlServer
{
    public class CTableTypeToPostgresTableTypeConverter : ICTableTypeToTableTypeConverter
    {



        public string Convert(CTableType tableType)
        {
            var codeWriter = new CodeWriter();
            codeWriter.WriteLine($@"DROP TYPE IF EXISTS {tableType.Schema.SchemaName.WrapReservedAndSnakeCase( DataStoreTypes.Postgres, tableType.ConvertToSnakeCase)}.{tableType.TableName.WrapReservedAndSnakeCase( DataStoreTypes.Postgres, tableType.ConvertToSnakeCase)} /* CASCADE */;");
            codeWriter.WriteLine();
            codeWriter.WriteLine($@"CREATE TYPE {tableType.Schema.SchemaName.WrapReservedAndSnakeCase(DataStoreTypes.Postgres, tableType.ConvertToSnakeCase)}.{tableType.TableName.WrapReservedAndSnakeCase(DataStoreTypes.Postgres, tableType.ConvertToSnakeCase)} AS (");
            codeWriter.Indent();

            var first = true;
            foreach (var col in tableType.Column)
            {
                if (!first)
                    codeWriter.WriteLine(",");
                first = false;

                codeWriter.Write($"{col.ColumnName.WrapReservedAndSnakeCase(DataStoreTypes.Postgres, tableType.ConvertToSnakeCase)}");


                codeWriter.Write($" {SqlMapper.NpgsqlDbTypeToPostgres(SqlMapper.DbTypeToNpgsqlDbType(col.ColumnType))}");
                if (col.DoesNeedLength())
                {
                    codeWriter.Write($"({col.ColumnLength})");
                }
            
                
            }

            codeWriter.Unindent();

            codeWriter.WriteLine(");");
            return codeWriter.ToString();
        }

        public static bool DoesNeedLength(SqlDbType columnSqlDbType)
        {
            //https://docs.microsoft.com/en-us/sql/t-sql/statements/create-type-transact-sql
            if (columnSqlDbType == SqlDbType.Char)
                return true;
            if (columnSqlDbType == SqlDbType.Binary)
                return true;
            if (columnSqlDbType == SqlDbType.NChar)
                return true;

            if (columnSqlDbType == SqlDbType.VarBinary)
                return true;
            if (columnSqlDbType == SqlDbType.VarChar)
                return true;

            return false;
        }

       
    }
}