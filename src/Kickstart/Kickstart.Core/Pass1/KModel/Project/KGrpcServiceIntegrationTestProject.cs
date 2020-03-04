using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcServiceIntegrationTestProject : KProject
    {
        public KGrpcServiceIntegrationTestProject()
        {
            ProjectIs = CProjectIs.Test;
            ProjectSuffix = "IntegrationTests.Api";
        }
    }
}