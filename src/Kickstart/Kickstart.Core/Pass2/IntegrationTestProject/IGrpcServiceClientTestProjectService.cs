using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.IntegrationTestProject
{
    public interface IGrpcServiceClientTestProjectService
    {
        CProject BuildProject(KSolution kSolution, KGrpcProject mGrpcProject, KGrpcServiceClientTestProject grpcMServiceClientTestProject, CProject grpcProject, bool addProtoRef = false);
    }
}