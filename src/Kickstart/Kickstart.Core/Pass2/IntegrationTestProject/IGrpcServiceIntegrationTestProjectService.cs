using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.IntegrationTestProject
{
    public interface IGrpcServiceIntegrationTestProjectService
    {
        CProject BuildProject(KGrpcProject mGrpcProject, KGrpcServiceIntegrationTestProject grpcMServiceIntegrationTestProject, CProject grpcIntegrationTestBusinessProject, bool addProtoRef = false);
    }
}