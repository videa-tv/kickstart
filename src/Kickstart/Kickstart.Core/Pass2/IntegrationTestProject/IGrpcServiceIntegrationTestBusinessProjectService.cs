using System.Collections.Generic;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.IntegrationTestProject
{
    public interface IGrpcServiceIntegrationTestBusinessProjectService
    {
        CProject BuildProject(KGrpcIntegrationBusinessTestProject mGrpcIntegrationBusinessTestProject, CProject grpcProject, IList<KProtoFile> protoFiles, bool addProtoRef = false);
    }
}