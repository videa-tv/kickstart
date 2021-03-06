using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CTableRowData : CPart
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

        public CColumn Column { get; set; }

        public object Value { get; set; }

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}