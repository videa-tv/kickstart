using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject.Table
{
    public class KViewToCTableConverter
    {
        #region Methods

        public CTable Convert(KView view)
        {
            var table = new CTable(DataStoreTypes.Unknown);
            table.Schema = view.Schema;
            table.Column = new List<CColumn>();
            table.TableName = view.ViewName;
            foreach (var col in view.Column)
            {
                var tableColumn = new CColumn(table)
                {
                    IsPrimaryKey = col.IsPrimaryKey,
                    IsIdentity = col.IsIdentity,
                    IsNullable = col.IsNullable,
                    IsIndexed = col.IsIndexed,
                    IsUnique = col.IsUnique,
                    ColumnName = col.ColumnName,
                    ColumnType = col.ColumnType,
                    ColumnSqlDbType = col.ColumnSqlDbType,
                    ColumnTypeRaw = col.ColumnTypeRaw,
                    ColumnLength = col.ColumnLength
                };

                if (col.ForeignKeyColumn != null)
                    foreach (var fk in col.ForeignKeyColumn)
                        tableColumn.ForeignKeyColumn.Add(
                            new CColumn(new CTable (DataStoreTypes.Unknown)
                            {
                                Schema = new CSchema {SchemaName = fk.View.Schema.SchemaName},
                                TableName = fk.View.ViewName
                            })
                            {
                                ColumnName = fk.ColumnName,
                                ColumnType = fk.ColumnType,
                                ColumnSqlDbType = fk.ColumnSqlDbType,
                                ColumnLength = fk.ColumnLength,
                                ColumnTypeRaw = fk.ColumnTypeRaw
                            }
                        );
                table.Column.Add(tableColumn);
            }
            foreach (var row in view.Row)
            {
                var tableRow = new CTableRow();
                foreach (var rowData in row.RowData)
                {
                    var tableRowData = new CTableRowData
                    {
                        Column = table.Column.FirstOrDefault(c => c.ColumnName == rowData.Column.ColumnName),
                        Value = rowData.Value
                    };
                    tableRow.RowData.Add(tableRowData);
                }
                table.Row.Add(tableRow);
            }
            return table;
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