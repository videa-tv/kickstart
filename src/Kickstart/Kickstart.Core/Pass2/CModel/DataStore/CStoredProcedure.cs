using System.Collections.Generic;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CStoredProcedure : CPart
    {
        #region Methods

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Properties

        public CSchema Schema { get; set; }

        public string StoredProcedureName { get; set; }

        /// <summary>
        ///     Used to produce documentation
        /// </summary>
        public string StoredProcedureDescription { get; set; }

        public List<CStoredProcedureParameter> Parameter { get; set; } = new List<CStoredProcedureParameter>();

        public bool ReturnsMultipleRows { get; set; }

        public bool HasResultSet => ResultSet.Count > 0;

        public string ResultSetName { get; set; }

        public List<CColumn> ResultSet { get; set; } = new List<CColumn>();

        public string StoredProcedureBody { get; set; }

        public string ParameterSetName { get; set; }

        public bool KickstartApi { get; set; } = false; // if true, will Kickstart Grpc, even if SP is CRUD
        public COperationIs DataOperationIs { get; set; }

        public DataStoreTypes DatabaseType { get; set; }

        public bool GenerateAsEmbeddedQuery { get; set; }
        //public string StoredProcedureNameSnakeCase { get { return StoredProcedureName.ToSnakeCase(); } }

        public bool ConvertToSnakeCase { get; internal set; }
        #endregion Properties

        #region Constructors
        public  CStoredProcedure(DataStoreTypes databaseType)
        {
            DatabaseType = databaseType;
       }
        #endregion Constructors
    }
}