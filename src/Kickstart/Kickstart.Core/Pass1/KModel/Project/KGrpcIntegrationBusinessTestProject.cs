using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcIntegrationBusinessTestProject : KProject
    {
        public KGrpcIntegrationBusinessTestProject()
        {
            ProjectIs = CProjectIs.Test;
            ProjectSuffix = "IntegrationTests.Business";
        }
    }
}