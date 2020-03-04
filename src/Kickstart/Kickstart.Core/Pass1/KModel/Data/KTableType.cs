using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass1.KModel
{
    public class KTableType : KPart
    {
        public string TableTypeText { get; set; }
        public string TableTypeName { get; set; }
        public string Schema { get; set; }
        public CTableType GeneratedTableType { get; set; }
    }
}