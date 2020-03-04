using Kickstart.Pass2.CModel.Code;
using Kickstart.Utility;

namespace Kickstart.Pass1.KModel
{
    public class KDataStoreTestProject : KProject
    {
        public KDataStoreTestProject()
        {
            ProjectIs = CProjectIs.Test;
            ProjectSuffix = "Db.Test";
        }
        public DataStoreTypes DataStoreType { get; set; } = DataStoreTypes.SqlServer;
    }
}