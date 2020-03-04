using System;
using System.Collections.Generic;
using Kickstart.Pass0.Model;
using Kickstart.Pass1.KModel;
using Kickstart.Services.Types;
using MediatR;

namespace Kickstart.Services.Query
{
    public class SplitPostgresDDLQuery : IRequest<KDataStoreProject>
    {
        public Utility.DataStoreTypes   DataStoreType { get; set; }
        public string UnSplitTableDDL { get; set; }
        public string UnSplitTableTypeDDL { get; set; }
        public string UnSplitStoredProcedureDDL { get; set; }
        public string UnSplitViewDDL { get; set; }
        public string UnSplitFunctionDDL { get; set; }
    }
}
