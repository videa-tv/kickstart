using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.SqlServer;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass1.Service
{
    public interface ISqlServerTableToCTableConverter
    {
        CTable Convert(DataTable dataTable, IEnumerable<TableForeignKeysDto> foreignKeys);
        void FixupForeignKeyTables(IEnumerable<CTable> tables);
    }

    public class SqlServerTableToCTableConverter : ISqlServerTableToCTableConverter
    {
        #region Methods

        public CTable Convert(DataTable dataTable, IEnumerable<TableForeignKeysDto> foreignKeys)
        {
            var table = new CTable( DataStoreTypes.SqlServer);
            table.Column = new List<CColumn>();

            foreach (var row2 in dataTable.Rows)
            {
                var row = row2 as DataRow;
                table.TableName = (string) row[4];
                table.Schema = new CSchema {SchemaName = (string) row[5]};

                var column = new CColumn(table)
                {
                    ColumnName = (string) row[0],
                    ColumnTypeRaw = (string) row[1],
                    ColumnSqlDbType = SqlMapper.ParseValueAsSqlDbType((string) row[1]),
                    ColumnType = SqlMapper.GetDbType((string) row[1]),
                    IsNullable = ((string)row[2]).ToLower() =="yes",
                    ColumnLength = row[3] != DBNull.Value ? (int) row[3] : -1,
                    IsPrimaryKey = (bool) row[6],
                    IsIdentity = (bool) row[7],
                    IsComputed = (bool) row[8]
                    
                };
                foreach (var fk in foreignKeys)
                {
                    if (fk.ColumnName == column.ColumnName)
                    {
                        var fkTable = new CTable(table.DatabaseType); //temp object, will do Fixup later to point to "real" table
                        fkTable.Schema = new CSchema {SchemaName = fk.ReferencedTableSchema};
                        fkTable.TableName = fk.ReferencedTableName;
                        column.ForeignKeyColumn.Add(new CColumn(fkTable)
                        {
                            ColumnName = fk.ReferencedColumnName,
                            ColumnType = column.ColumnType,
                            ColumnTypeRaw = column.ColumnTypeRaw
                        });
                    }
                }
                table.Column.Add(column);
            }
            return table;
        }

        public void FixupForeignKeyTables(IEnumerable<CTable> tables)
        {
            foreach (var table in tables)
            {
                foreach (var cColumn in table.Column)
                {
                    foreach (var fkCColumn in cColumn.ForeignKeyColumn)
                    {
                        var realTable = tables.First(t =>
                            t.Schema.SchemaName == fkCColumn.Table.Schema.SchemaName &&
                            t.TableName == fkCColumn.Table.TableName);

                        fkCColumn.FixupTable(realTable);
                    }
                }
            }
        }
        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}