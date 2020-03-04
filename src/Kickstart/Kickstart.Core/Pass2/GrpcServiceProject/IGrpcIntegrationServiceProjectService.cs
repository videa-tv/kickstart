using System.Collections.Generic;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface IGrpcIntegrationServiceProjectService
    {
        CProject BuildProject(KSolution mSolution, KGrpcIntegrationProject grpcKIntegrationProject, IList<KProtoRef> protoRpcRefs);
    }
}