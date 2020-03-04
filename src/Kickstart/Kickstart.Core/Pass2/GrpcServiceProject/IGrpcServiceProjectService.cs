using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface IGrpcServiceProjectService
    {
        CProject BuildProject(KSolution kSolution, KDataStoreProject databaseKProject, KDataLayerProject dataLayerKProject, KGrpcProject grpcKProject, CProject sqlProject, CProject dataProject);
    }
}