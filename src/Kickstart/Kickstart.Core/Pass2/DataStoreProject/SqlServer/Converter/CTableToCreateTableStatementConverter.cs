using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.SqlServer
{
    class CTableToCreateTableStatementConverter
    {
        public CreateTableStatement Convert(CTable table)
        {
            var parser = new TSql120Parser(false);
            string[] parts = { table.Schema.SchemaName, table.TableName };

            var createTable = new CreateTableStatement();

            ///set schema and table name
            createTable.SchemaObjectName = new SchemaObjectName();

            createTable.SchemaObjectName.Identifiers.Add(new Identifier { Value = table.Schema.SchemaName });
            createTable.SchemaObjectName.Identifiers.Add(new Identifier { Value = $"[{table.TableName}]" });

            //add columns
            createTable.Definition = new TableDefinition();

            foreach (var col in table.Column)
            {
                if (col.ColumnType == DbType.Object)
                {
                    continue;
                }
                var dataType = new SqlDataTypeReference
                {
                    SqlDataTypeOption = SqlMapper.SqlTypeToSqlDataTypeOption(col.ColumnTypeRaw)
                };
                if (col.ColumnLength > 0)
                    dataType.Parameters.Add(new IntegerLiteral { Value = col.ColumnLength.ToString() });
                var column = new ColumnDefinition
                {
                    ColumnIdentifier = new Identifier { Value = col.ColumnName.WrapReservedAndSnakeCase(DataStoreTypes.SqlServer, table.ConvertToSnakeCase) },
                    DataType = dataType
                };
                if (!string.IsNullOrEmpty(col.DefaultValue))
                {
                    IList<ParseError> errors;
                    //var defaultValueText = "CONVERT([char](32), REPLACE(CONVERT([char](36),NEWID()),'-',''))";
                    var scriptDefault = parser.ParseExpression(new StringReader(col.DefaultValue), out errors);
                    column.DefaultConstraint = new DefaultConstraintDefinition { Expression = scriptDefault };
                }

                if (col.IsIdentity)
                    column.IdentityOptions = new IdentityOptions
                    {
                        IdentitySeed = new IntegerLiteral { Value = col.ColumnType == System.Data.DbType.Byte ? "1" : "1000" },
                        IdentityIncrement = new IntegerLiteral { Value = "1" }
                    };
                column.Constraints.Add(new NullableConstraintDefinition { Nullable = col.IsNullable });
                if (col.IsUnique)
                    column.Constraints.Add(new UniqueConstraintDefinition());
                if (col.IsIndexed)
                    column.Index = new IndexDefinition
                    {
                        Name = new Identifier { Value = $"IX_{col.ColumnName}" },
                        IndexType = new IndexType
                        {
                            IndexTypeKind = IndexTypeKind.NonClustered
                        }
                    };
                createTable.Definition.ColumnDefinitions.Add(column);
            }
            //add PK's
            var pks = table.GetPrimaryKeyColumns();
            if (pks.Count > 0)
            {
                    var primaryKeyConstraint = new UniqueConstraintDefinition { IsPrimaryKey = true };
                    primaryKeyConstraint.Clustered = true; // todo: use metadata
                    foreach (var pk in table.GetPrimaryKeyColumns())
                    {

                        var columnIdentifier = new MultiPartIdentifier();
                        columnIdentifier.Identifiers.Add(new Identifier { Value = pk.ColumnName });
                        var columnRefExpression = new ColumnReferenceExpression
                        {
                            MultiPartIdentifier = columnIdentifier
                        };

                        var columnWithSortOrder = new ColumnWithSortOrder { Column = columnRefExpression };
                        primaryKeyConstraint.Columns.Add(columnWithSortOrder);
                    }

                    var pkConstraintName = $"PK_{table.Schema.SchemaName}_{table.TableName}_{pks.First().ColumnName}";

                    primaryKeyConstraint.ConstraintIdentifier = new Identifier { Value = pkConstraintName };
                    createTable.Definition.TableConstraints.Add(primaryKeyConstraint);
            }

            //add foreign keys
            foreach (var col in table.Column)
            {
                if (col.ForeignKeyColumn == null || col.ForeignKeyColumn.Count == 0)
                    continue;
                foreach (var fk in col.ForeignKeyColumn)
                {
                    var fkConstraintName = $"FK_{table.TableName}_{fk.Table.TableName}";
                    var foreignKeyConstraint = new ForeignKeyConstraintDefinition();
                    foreignKeyConstraint.ConstraintIdentifier = new Identifier { Value = fkConstraintName };
                    foreignKeyConstraint.Columns.Add(new Identifier { Value = col.ColumnName });
                    foreignKeyConstraint.ReferenceTableName = new SchemaObjectName();
                    foreignKeyConstraint.ReferenceTableName.Identifiers.Add(new Identifier
                    {
                        Value = fk.Table.Schema.SchemaName
                    });
                    foreignKeyConstraint.ReferenceTableName.Identifiers.Add(new Identifier
                    {
                        Value = fk.Table.TableName
                    });

                    foreignKeyConstraint.ReferencedTableColumns.Add(new Identifier { Value = fk.ColumnName });
                    createTable.Definition.TableConstraints.Add(foreignKeyConstraint);
                }
            }
            return createTable;

        }
    }
}
