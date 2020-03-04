using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Utility;

namespace Kickstart.Pass1.KModel.Data
{
    public class KFunction : CFunction
    {
        public CFunction GeneratedFunction { get; set; }

        public KFunction(DataStoreTypes databaseType) : base(databaseType)
        {
        }
    }
}
