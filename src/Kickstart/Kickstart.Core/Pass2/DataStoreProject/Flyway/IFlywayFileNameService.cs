using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Pass2.DataStoreProject
{
    public interface IFlywayFileNameService
    {
        string GetFlywayFileName(CTableType tableType);

        string GetFlywayFileName(CTable table);

        string GetFlywayFileName(CSchema schema);

        string GetFlywayFileName(CView view);
        string GetFlywayFileName(CFunction function);


        string GetFlywayFileName(CStoredProcedure storedProcedure);


    }
}