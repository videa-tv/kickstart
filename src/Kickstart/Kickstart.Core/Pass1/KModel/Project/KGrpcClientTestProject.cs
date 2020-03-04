using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcServiceClientTestProject : KProject
    {
        public KGrpcServiceClientTestProject()
        {
            ProjectIs = CProjectIs.Client | CProjectIs.Test | CProjectIs.Service;
            ProjectSuffix = "Services.ClientTest";
        }
    }
}