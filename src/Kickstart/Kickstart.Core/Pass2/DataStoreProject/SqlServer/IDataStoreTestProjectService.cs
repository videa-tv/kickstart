using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.DataStoreProject.SqlServer
{
    public interface IDataStoreTestProjectService
    {
        CProject BuildProject(KSolution kSolution, KDataStoreProject dataStoreKProject, KDataStoreTestProject sqlTestKProject, KDataLayerProject dataLayerKProject, CProject dataStoreProject, CInterface dbProviderInterface, CClass dbProviderClass);
    }
}
