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

namespace Kickstart.Services.Query
{
    public class SplitSqlServerDDLHandler : IRequestHandler<SplitSqlServerDDLQuery, KDataStoreProject>
    {

        private readonly IDbToKSolutionConverter _dbToKSolutionConverter;

        public SplitSqlServerDDLHandler(IDbToKSolutionConverter dbToKSolutionConverter)
        {

            _dbToKSolutionConverter = dbToKSolutionConverter;
        }
        public async Task<KDataStoreProject> Handle(SplitSqlServerDDLQuery query, CancellationToken cancellationToken)
        {
            var databaseProject = new KDataStoreProject()
            {
                KickstartCRUDStoredProcedures = false,
                ConvertToSnakeCase = false,// query.ConvertToSnakeCase,
                DataStoreType = Utility.DataStoreTypes.SqlServer,
                SqlTableText = query.UnSplitTableDDL,
                SqlTableTypeText = query.UnSplitTableTypeDDL,
                SqlViewText =  query.UnSplitViewDDL,
                SqlFunctionText =  query.UnSplitFunctionDDL,
                SqlStoredProcedureText = query.UnSplitStoredProcedureDDL
            };

            var connectionString = "Server=localhost;";
            var outputRootPath = string.Empty;

            return _dbToKSolutionConverter.BuildSqlMeta(connectionString, outputRootPath, databaseProject);
            
        }
        
    }
}
