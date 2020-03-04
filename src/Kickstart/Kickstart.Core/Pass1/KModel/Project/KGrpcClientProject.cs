using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcClientProject : KProject
    {
        public KGrpcClientProject()
        {
            ProjectIs = CProjectIs.Client ;
            ProjectSuffix = "GrpcClients";
        }
    }
}