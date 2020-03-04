using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MediatR;
using Grpc.Core;
using Kickstart.Services.Types;
using Kickstart.Wizard.Service;
using Kickstart.Pass1.Service;
using Kickstart.Pass3.gRPC;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass1;
using Kickstart.Pass2.DataLayerProject;
using Microsoft.Extensions.Configuration;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.SqlServer;
using StructureMap;
using Kickstart.Pass2.DataStoreProject;
using Kickstart.Pass2.CModel.DataStore;

namespace Kickstart.Services.Query
{
    public class SplitPostgresDDLHandler : IRequestHandler<SplitPostgresDDLQuery, KDataStoreProject>
    {

        private readonly IFlywayFileNameService _flywayFileNameService;

        public SplitPostgresDDLHandler(IFlywayFileNameService flywayFileNameService)
        {
            _flywayFileNameService = flywayFileNameService;

        }


        public async Task<KDataStoreProject> Handle(SplitPostgresDDLQuery query, CancellationToken cancellationToken)
        {
            var dataStoreProject = new KDataStoreProject();
            const string createOrReplace = "CREATE OR REPLACE FUNCTION "; //todo: needs to be based on DBMS

            var storedProcs = query.UnSplitStoredProcedureDDL.Split(new[] { createOrReplace }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var storedProc in storedProcs)
            {
                var spFullName = storedProc.Substring(0, storedProc.IndexOf("("));
                var schemaName = spFullName.Substring(0, spFullName.IndexOf("."));
                var spName = spFullName.Substring(spFullName.IndexOf(".") + 1);
                var fileName = _flywayFileNameService.GetFlywayFileName(new CStoredProcedure(Utility.DataStoreTypes.Postgres)
                {
                    Schema = new CSchema { SchemaName = schemaName },
                    StoredProcedureName = spName
                });

                dataStoreProject.StoredProcedure.Add(new KStoredProcedure { StoredProcedureName = spName, Schema = schemaName, StoredProcedureText = createOrReplace + storedProc });
            }

            return dataStoreProject;
        }
        
    }
}
