using System.Collections.Generic;
using System.Data;
using Kickstart.Interface;
using Kickstart.Pass1.KModel;
using Newtonsoft.Json;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CColumn : CPart
    {
        public override string ToString()
        {
            return $"{ColumnName} {ColumnType}";
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
        public bool DoesNeedLength()
        {
            //https://docs.microsoft.com/en-us/sql/t-sql/statements/create-type-transact-sql
            if (this.ColumnType == DbType.AnsiStringFixedLength)
                return ColumnLength > 0;
            if (ColumnType == DbType.Binary)
                return ColumnLength > 0;
            if (ColumnType == DbType.StringFixedLength)
                return ColumnLength > 0;
            
            if (ColumnType == DbType.String)
                return ColumnLength > 0 && ColumnLength <= 8000; 
            if (ColumnType == DbType.AnsiString)
                return ColumnLength > 0;

            return false;
        }

        

        private CStoredProcedure _storedProcedure;

        public CColumn(CTable table)
        {
            Table = table;
        }

        public CColumn(CFunction function)
        {
            Function = function;
        }

        public CColumn(CView view)
        {
            View = view;
        }

        public CColumn(CStoredProcedure storedProcedure)
        {
            _storedProcedure = storedProcedure;
        }



        public string ColumnTypeRaw { get; set; }

        public DbType ColumnType { get; set; }

        public SqlDbType ColumnSqlDbType { get; set; }

        public string ColumnNameOriginal { get; private set; }
        string _columnName;
        private CFunction Function;

        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(ColumnNameOriginal) && !string.IsNullOrWhiteSpace(value))
                    ColumnNameOriginal = value;
                
                _columnName = value;
            }
        }


        /// <summary>
        ///     Used to produce documentation
        /// </summary>
        public string ColumnDescription { get; set; }

        public int ColumnLength { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsNullable { get; set; } = true;

        public bool IsIndexed { get; set; } = false;
        public bool IsUnique { get; set; } = false;
        public bool IsComputed { get; set; } = false;

        public bool IsCreatedDate => string.Compare(ColumnName, "CreatedDateUtc", true) == 0;

        public bool IsModifiedDate => string.Compare(ColumnName, "ModifiedDateUtc", true) == 0;

        public string DefaultValue { get; set; }

        public List<CColumn> ForeignKeyColumn { get; set; } = new List<CColumn>();

        [JsonIgnore]
        public CView View { get; }

        [JsonIgnore]
        public CTable Table { get; private set; }

        public bool IsRowVersion => ColumnSqlDbType == SqlDbType.Timestamp;

        //public string ColumnNameSnakeCase { get { return ColumnName.ToSnakeCase(); } }


        public void FixupTable(CTable realTable)
        {
            Table = realTable;
        }
    }
}