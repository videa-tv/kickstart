using System;
using System.Collections.Generic;
using System.Data;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass1.Service
{
    /*
    public interface ISqlServerFunctionToCFunctionConverter
    {
        CFunction Convert(DataTable dataTable);
    }

    public class SqlServerFunctionToCFunctionConverter : ISqlServerFunctionToCFunctionConverter

    {
        #region Methods

        public CView Convert(DataTable dataTable)
        {
            var view = new CView();
            view.Column = new List<CColumn>();

            foreach (var row2 in dataTable.Rows)
            {
                var row = row2 as DataRow;
                view.ViewName = (string) row[4];
                view.Schema = new CSchema {SchemaName = (string) row[5]};

                var column = new CColumn(view)
                {
                    ColumnName = (string) row[0],
                    ColumnTypeRaw = (string) row[1],
                    ColumnSqlDbType = SqlMapper.ParseValueAsSqlDbType((string) row[1]),
                    ColumnType = SqlMapper.GetDbType((string) row[1]),
                    ColumnLength = row[3] != DBNull.Value ? (int) row[3] : -1,
                    ColumnDescription = row[6] != DBNull.Value ? (string) row[6] : null
                };
                view.Column.Add(column);
            }
            return view;
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
    */
}