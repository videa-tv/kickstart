using System.Collections.Generic;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass2.DataLayerProject
{
    public interface IRelationalDataLayerServiceProjectService
    {
        string ConnectionString { get; set; }
        DataStoreTypes ConnectsToDatabaseType { get; set; }

        CInterface BuildIDbDiagnosticsFactoryInterface(CProject dataProject);
        CProject BuildProject(KDataStoreProject databaseKProject, KDataLayerProject dataLayerKProject, IEnumerable<CStoredProcedure> storedProceduresIn, IEnumerable<KTable> tables, IEnumerable<KTableType> tableTypes, IEnumerable<CView> views);
    }
}