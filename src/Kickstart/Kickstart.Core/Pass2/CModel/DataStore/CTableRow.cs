using System.Collections.Generic;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CTableRow : CPart
    {
        #region Properties

        public List<CTableRowData> RowData { get; set; } = new List<CTableRowData>();

        #endregion Properties

        #region Methods

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        #endregion Methods

        #region Fields

        #endregion Fields

        #region Constructors

        #endregion Constructors
    }
}