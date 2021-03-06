using System;
using System.Data;
using Kickstart.Pass2.CModel.DataStore;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass3.SqlServer
{
    public class STableToSqlServerTableConverter
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
            string[] parts = {table.Schema.SchemaName, table.TableName};

            var createTable = new CreateTableStatement();

            ///set schema and table name
            createTable.SchemaObjectName = new SchemaObjectName();

            createTable.SchemaObjectName.Identifiers.Add(new Identifier {Value = table.Schema.SchemaName});
            createTable.SchemaObjectName.Identifiers.Add(new Identifier {Value = table.TableName});

            //add columns
            createTable.Definition = new TableDefinition();

            foreach (var col in table.Column)
            {
                var dataType = new SqlDataTypeReference {SqlDataTypeOption = GetSqlDataTypeOption(col.ColumnType)};
                if (col.ColumnLength > 0)
                    dataType.Parameters.Add(new IntegerLiteral {Value = col.ColumnLength.ToString()});
                var column = new ColumnDefinition
                {
                    ColumnIdentifier = new Identifier {Value = col.ColumnName},
                    DataType = dataType
                };

                createTable.Definition.ColumnDefinitions.Add(column);
            }

            //generate DDL
            var script = new TSqlScript();
            var batch = new TSqlBatch();
            script.Batches.Add(batch);
            batch.Statements.Add(createTable);
            var dacpacModel = new TSqlModel(SqlServerVersion.Sql120, new TSqlModelOptions());
            dacpacModel.AddObjects(script);
            var existing = dacpacModel.GetObject(Table.TypeClass, new ObjectIdentifier(parts), DacQueryScopes.All);
            return existing.GetScript();
        }

        public SqlDataTypeOption GetSqlDataTypeOption(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Byte:
                    return SqlDataTypeOption.TinyInt;
                case DbType.SByte:
                    return SqlDataTypeOption.TinyInt;
                case DbType.Int16:
                    return SqlDataTypeOption.SmallInt;
                case DbType.UInt16:
                    return SqlDataTypeOption.SmallInt;
                case DbType.Int32:
                    return SqlDataTypeOption.Int;
                case DbType.UInt32:
                    return SqlDataTypeOption.Int;
                case DbType.Int64:
                    return SqlDataTypeOption.BigInt;
                case DbType.UInt64:
                    return SqlDataTypeOption.BigInt;
                case DbType.Single:
                    return SqlDataTypeOption.Real;
                case DbType.Double:
                    return SqlDataTypeOption.Float;
                case DbType.Decimal:
                    return SqlDataTypeOption.Decimal;
                case DbType.Boolean:
                    return SqlDataTypeOption.Bit;
                case DbType.String:
                    return SqlDataTypeOption.NVarChar;
                case DbType.AnsiString:
                    return SqlDataTypeOption.VarChar;
                case DbType.StringFixedLength:
                    return SqlDataTypeOption.NChar;
                case DbType.AnsiStringFixedLength:
                    return SqlDataTypeOption.Char;
                case DbType.Date:
                    return SqlDataTypeOption.Date;
                case DbType.Time:
                    return SqlDataTypeOption.Time;
                case DbType.DateTime:
                    return SqlDataTypeOption.DateTime;
                case DbType.DateTime2:
                    return SqlDataTypeOption.DateTime2;
                case DbType.DateTimeOffset:
                    return SqlDataTypeOption.DateTimeOffset;
                case DbType.Xml:
                    return SqlDataTypeOption.Text;
                case DbType.Guid:
                    return SqlDataTypeOption.UniqueIdentifier;
                case DbType.Object:
                    return SqlDataTypeOption.Sql_Variant;
                case DbType.Binary:
                    return SqlDataTypeOption.VarBinary;
                case DbType.Currency:
                    return SqlDataTypeOption.Money;
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion Methods
    }
}