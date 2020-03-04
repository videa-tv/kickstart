using Kickstart.Pass2.SqlProject;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataStoreProject
{
    public interface IDataStoreServiceFactory
    {
        IDataStoreProjectService Create(DataStoreTypes dataStoreType);
    }
}