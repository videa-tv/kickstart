using System.Collections.Generic;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CQuery : CPart
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

        public string QueryName { get; set; }

        /// <summary>
        ///     Used to produce documentation
        /// </summary>
        public string QueryDescription { get; set; }

        public List<CStoredProcedureParameter> Parameter { get; set; } = new List<CStoredProcedureParameter>();

        public bool ReturnsMultipleRows { get; set; }

        public bool HasResultSet => ResultSet.Count > 0;

        public string ResultSetName { get; set; }

        public List<CColumn> ResultSet { get; set; } = new List<CColumn>();

        public string QueryBody { get; set; }

        public string ParameterSetName { get; set; }

        public bool KickstartApi { get; set; } = false; // if true, will Kickstart Grpc, even if SP is CRUD
        public COperationIs DataOperationIs { get; set; }

        #endregion Properties

        #region Constructors
        public  CQuery()
        {
            int x = 1;
       }
        #endregion Constructors
    }
}