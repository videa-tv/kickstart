using System.Collections.Generic;
using Kickstart.Pass1.KModel;

namespace Kickstart.Interface
{
    public interface IMetadataConfigService
    {
        void ConfigureMetaData(IEnumerable<KView> views, List<KStoredProcedure> storedProcedures, List<KTable> tables);
        void ConfigureMetaData2(IEnumerable<KView> views, List<KStoredProcedure> storedProcedures, List<KTable> tables);
    }
}