using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject
{
    public interface IDataLayerServiceFactory
    {
        IDataLayerServiceProjectService Create(DataStoreTypes dataStoreType);
    }
}