using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.SqlProject
{
    public interface IDataStoreProjectService
    {
        CProject BuildProject(KSolution mSolution, KDataStoreProject sqlKProject);
    }
}