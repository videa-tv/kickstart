using Kickstart.Interface;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CSchema : CPart
    {
        #region Properties

        public string SchemaNameOriginal { get; private set; }
        private string _schemaName;
        public string SchemaName {
            get { return _schemaName; }
            set
            {
                if (string.IsNullOrWhiteSpace(SchemaNameOriginal) && !string.IsNullOrWhiteSpace(value))
                    SchemaNameOriginal = value;

                _schemaName = value;
            }
        }

        
        public DataStoreTypes DatabaseType { get; internal set; }
        //public string SchemaNameSnakeCase { get { return SchemaName.ToSnakeCase(); } }

        public bool ConvertToSnakeCase { get; internal set; }

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
        /*
        public CSchema (DatabaseTypes databaseType)
        {
            DatabaseType = databaseType;
        }*/
        #endregion Constructors
    }
}