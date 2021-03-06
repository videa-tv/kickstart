using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kickstart.Interface;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CTable : CPart
    {
  
        
        public CSchema Schema { get; set; }

        private string _tableName;
        public string TableNameOriginal { get; private set; }

        public string TableName
        {
            get { return _tableName; }
            set
            {
                if (string.IsNullOrWhiteSpace(TableNameOriginal) && !string.IsNullOrWhiteSpace(value))
                    TableNameOriginal = value;

                _tableName = value;
            }
        }

        public List<CColumn> Column { get; set; } = new List<CColumn>();

        public List<CTableRow> Row { get; set; } = new List<CTableRow>();

        public CStoredProcedure InsertStoredProcedure { get; set; }

        public string TableText { get; set; }


        public DataStoreTypes DatabaseType { get; set; }
        //public string TableNameSnakeCase { get { return TableName.ToSnakeCase(); } }

        public bool ConvertToSnakeCase { get; internal set; }

        
        
        public CTable(DataStoreTypes databaseType)
        {
            DatabaseType = databaseType;
        }

        public override string ToString()
        {
            return $"{Schema.SchemaName}.{TableName}";
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public List<CColumn> GetPrimaryKeyColumns()
        {
            return Column.FindAll(c => c.IsPrimaryKey);
        }

        public CColumn GetRowVersionColumn()
        {
            return Column.FirstOrDefault(c => c.IsRowVersion);
        }

        internal string ColumnAsColumnList()
        {
            var columns = string.Empty;
            var scalerCols = Column.Where(c => c.ColumnType != DbType.Object); //ignore table types

            return string.Join(",", scalerCols.Select(c => c.ColumnName));
        }

        internal bool ColumnExists(CColumn col)
        {
            var existingCol = this.Column.FirstOrDefault(c => c.ColumnName.ToLower() == col.ColumnName.ToLower());
            if (existingCol == null)
                return false;

            if (existingCol.ColumnType != col.ColumnType)
            {
                if (existingCol.ColumnType == DbType.Int64 && existingCol.IsPrimaryKey)
                {
                    var newName = existingCol.ColumnName += "Key";
                    if (this.Column.Exists(c=>c.ColumnName.ToLower() == newName.ToLower()))
                    {
                        return true;
                    }

                    //change the auto created PK id to not colide
                    existingCol.ColumnName += "Key";
                    return false;
                }
                else if (col.ColumnType == DbType.Int64 && col.IsPrimaryKey)
                {
                    col.ColumnName += "Key";
                }
                else
                {
                    throw new ApplicationException("Attempting to create 2 different columns with same name");
                }
            }

            return true;
        }
        
    }
}