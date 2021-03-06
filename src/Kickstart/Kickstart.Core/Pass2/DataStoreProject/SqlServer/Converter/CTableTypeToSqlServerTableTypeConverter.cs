using System.Data;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Kickstart.Pass2.SqlServer
{
    public class CTableTypeToSqlServerTableTypeConverter : ICTableTypeToTableTypeConverter
    {
        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors

        #region Methods

        public string Convert(CTableType tableType)
        {
            string[] parts = {tableType.Schema.SchemaName, tableType.TableName};

            var createTypeTable = new CreateTypeTableStatement();

            ///set schema and table name
            createTypeTable.Name = new SchemaObjectName();

            createTypeTable.Name.Identifiers.Add(new Identifier {Value = tableType.Schema.SchemaName});
            createTypeTable.Name.Identifiers.Add(new Identifier {Value = tableType.TableName});

            //add columns
            createTypeTable.Definition = new TableDefinition();

            foreach (var col in tableType.Column)
            {
                var dataType = new SqlDataTypeReference
                {
                    SqlDataTypeOption = SqlMapper.SqlTypeToSqlDataTypeOption(col.ColumnTypeRaw)
                };
                if (DoesNeedLength(col.ColumnSqlDbType))
                    if (col.ColumnLength > 0)
                        dataType.Parameters.Add(new IntegerLiteral {Value = col.ColumnLength.ToString()});
                var column = new ColumnDefinition
                {
                    ColumnIdentifier = new Identifier {Value = col.ColumnName},
                    DataType = dataType
                };
                if (col.IsIdentity)
                    column.IdentityOptions = new IdentityOptions
                    {
                        IdentitySeed = new IntegerLiteral {Value = "1000"},
                        IdentityIncrement = new IntegerLiteral {Value = "1"}
                    };
                column.Constraints.Add(new NullableConstraintDefinition {Nullable = col.IsNullable});
                if (col.IsUnique)
                    column.Constraints.Add(new UniqueConstraintDefinition());
                if (col.IsIndexed)
                    column.Index = new IndexDefinition
                    {
                        Name = new Identifier {Value = $"IX_{col.ColumnName}"},
                        IndexType = new IndexType
                        {
                            IndexTypeKind = IndexTypeKind.NonClustered
                        }
                    };
                createTypeTable.Definition.ColumnDefinitions.Add(column);
            }


            /*
            //generate DDL
            var script = new TSqlScript();
            var batch = new TSqlBatch();
            script.Batches.Add(batch);
            batch.Statements.Add(createTypeTable);
            var dacpacModel = new TSqlModel(SqlServerVersion.Sql120, new TSqlModelOptions());
            dacpacModel.AddObjects(script);
            var existing = dacpacModel.GetObject(Table.TypeClass, new ObjectIdentifier(parts), DacQueryScopes.All);
            return existing.GetScript();
            */
            string scriptOut;
            var scriptGen = new Sql120ScriptGenerator();

            scriptGen.GenerateScript(createTypeTable, out scriptOut);
            return scriptOut;
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

        #endregion Methods
    }
}