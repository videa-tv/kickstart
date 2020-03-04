using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.AWS.DMS;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.SqlServer;
using Kickstart.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.DataStoreProject
{
    public class GenericResourceProjectService : IGenericResourceProjectService
    {
        public CProject BuildProject(KSolution kSolution, KDataStoreProject dataStoreKProject)
        {
            return new CProject { Kickstart = false };
            
        }
        

    }
}
