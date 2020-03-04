using Kickstart.Pass1.KModel;

namespace Kickstart.Pass1
{
    public interface IDbToKSolutionConverter
    {
        KDataStoreProject BuildSqlMeta(string connectionString, string outputRootPath, KDataStoreProject databaseProject);
    }
}