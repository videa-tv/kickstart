using System.Collections.Generic;
using Kickstart.Interface;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CView : CPart
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

        public string ViewName { get; set; }

        public List<CColumn> Column { get; set; } = new List<CColumn>();

        public string ViewText { get; set; }

        public string ViewQueryText { get; set; }

        public List<CTableRow> Row { get; set; } = new List<CTableRow>();
        //public string ViewNameSnakeCase { get { return ViewName.ToSnakeCase(); } }

        public DataStoreTypes DatabaseType { get; internal set; }
        public bool ConvertToSnakeCase { get; internal set; }

        // public bool KickstartApi { get; set; } = true; //if view is turned to table, and becomes CRUD stored proc, expose it 

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}