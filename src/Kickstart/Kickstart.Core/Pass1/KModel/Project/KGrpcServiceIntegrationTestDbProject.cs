using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcServiceIntegrationTestDbProject : KProject
    {
        public KGrpcServiceIntegrationTestDbProject()
        {
            ProjectIs = CProjectIs.Test;
            ProjectSuffix = "IntegrationTests.Persistence";
        }
    }
}