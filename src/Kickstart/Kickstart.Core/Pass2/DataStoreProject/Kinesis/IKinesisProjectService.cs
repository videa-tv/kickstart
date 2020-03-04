using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.SqlProject;

namespace Kickstart.Pass2.DataStoreProject
{
    public interface IKinesisProjectService : IDataStoreProjectService
    {
        CProject BuildProject(KSolution kSolution, KDataStoreProject sqlKProject);
    }
}