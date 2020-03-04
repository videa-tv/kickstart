using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject
{
    public interface IGenericResourceProjectService
    {
        CProject BuildProject(KSolution kSolution, KDataStoreProject sqlKProject);
    }
}