using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcServiceUnitTestProject : KProject
    {
        public KGrpcServiceUnitTestProject()
        {
            ProjectIs = CProjectIs.Test;
            ProjectSuffix = "Services.Tests";
        }
    }
}