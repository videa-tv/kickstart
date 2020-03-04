using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcIntegrationServiceClientTestProject : KProject
    {
        public KGrpcIntegrationServiceClientTestProject()
        {
            ProjectIs = CProjectIs.Test;
            ProjectSuffix = "Service.ClientTest";
        }
    }
}