using System.Collections.Generic;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Utility;

namespace Kickstart.Pass1.KModel
{
    public class KDataLayerProject : KProject
    {
        public KDataLayerProject(DataStoreTypes connectsToDatabaseType)
        {
            ProjectIs = CProjectIs.DataAccess;
            ProjectSuffix = $"Data";
            ProjectSuffixSuffix = $"{connectsToDatabaseType}";

            ConnectsToDatabaseType = connectsToDatabaseType;
        }

        public DataStoreTypes ConnectsToDatabaseType { get; set; } = DataStoreTypes.SqlServer;

        public bool KickstartBulkStore { get; set; } = true;
        public string BulkStoreMethodName { get; set; } = "BulkStore";

        public IList<CInterface> DataServiceInterface { get; set; } = new List<CInterface>();


        /// <summary>
        ///     create separate data access class per table
        /// </summary>
        public bool OnePerTable { get; set; } = false;
        
    }
}