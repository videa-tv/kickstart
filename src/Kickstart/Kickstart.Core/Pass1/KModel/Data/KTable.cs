using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass1.KModel
{
    public class KTable : CTable
    {
        public CTable GeneratedTable { get; set; }
        public string SampleDataExcelFile { get; set; }

        public KTable() : base(DataStoreTypes.Unknown)
        {

        }
    }
}