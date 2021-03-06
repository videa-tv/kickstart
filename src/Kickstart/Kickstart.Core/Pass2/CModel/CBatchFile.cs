using Kickstart.Interface;

namespace Kickstart.Pass2.CModel
{
    public class CBatchFile : CPart
    {
        #region Properties

        public string BatchFileContent { get; set; } = "";

        public bool ExecutePostKickstart { get; set; }
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