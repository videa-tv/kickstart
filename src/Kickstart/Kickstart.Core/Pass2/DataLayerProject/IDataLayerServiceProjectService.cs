using System.Collections.Generic;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject
{
    public interface IDataLayerServiceProjectService
    {
        string ConnectionString { get; set; }
        DataStoreTypes ConnectsToDatabaseType { get; set; }

        CProject BuildProject(KDataStoreProject databaseKProject, KDataLayerProject dataLayerKProject, IEnumerable<CStoredProcedure> storedProceduresIn, IEnumerable<KTable> tables, IEnumerable<KTableType> tableTypes, IEnumerable<CView> views);
    }
}