using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CSeedScript : CPart
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

        public string SeedScriptName { get; set; }

        public string SeedScriptBody { get; set; }

        #endregion Properties

        #region Constructors

        #endregion Constructors
    }
}