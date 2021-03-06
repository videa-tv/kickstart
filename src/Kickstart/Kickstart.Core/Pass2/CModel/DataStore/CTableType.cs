using Kickstart.Interface;
using Kickstart.Utility;

namespace Kickstart.Pass2.CModel.DataStore
{
    public class CTableType : CTable
    {
        
        public CTableType(DataStoreTypes databaseType) : base(databaseType)
        {

        }

        public override string ToString()
        {
            return $"{Schema.SchemaName}.{TableName}";
        }
        
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string TableTypeBody { get; set; }
        //public string TableNameSnakeCase { get { return TableName.ToSnakeCase(); } }
    }
}