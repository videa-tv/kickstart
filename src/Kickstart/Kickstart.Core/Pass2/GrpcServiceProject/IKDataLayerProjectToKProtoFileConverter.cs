using System.Collections.Generic;
using Kickstart.Pass1.KModel;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface IKDataLayerProjectToKProtoFileConverter
    {
        IList<KProtoFile> Convert(KDataStoreProject databaseProject, KDataLayerProject dataLayerProject, KGrpcProject grpcKProject);
    }
}