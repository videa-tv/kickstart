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
    public class BuildSolutionHandler : IRequestHandler<BuildSolutionQuery, bool>
    {
        private readonly IKickstartWizardService _dataService;
        private readonly IKickstartService _kickstartService;
        private readonly IContainer _container;
        private readonly IDbToKSolutionConverter _dbToKSolutionConverter;
        private readonly ISProtoFileToProtoFileConverter _sProtoFileToProtoFileConverter;
        private readonly IKDataLayerProjectToKProtoFileConverter _kDataLayerProjectToKProtoFileConverter;
        private readonly IDataLayerServiceFactory _dataLayerServiceFactory;
        private readonly IProtoToKProtoConverter _protoToKProtoConverter;

        public BuildSolutionHandler(IKickstartWizardService dataService, IKickstartService kickstartService, IContainer container, IDbToKSolutionConverter dbToKSolutionConverter, 
            IProtoToKProtoConverter protoToKProtoConverter, ISProtoFileToProtoFileConverter sProtoFileToProtoFileConverter, IKDataLayerProjectToKProtoFileConverter kDataLayerProjectToKProtoFileConverter, IDataLayerServiceFactory dataLayerServiceFactory)
        {
            _dataService = dataService;
            _kickstartService = kickstartService;
            _container = container;
            _dbToKSolutionConverter = dbToKSolutionConverter;
            _sProtoFileToProtoFileConverter = sProtoFileToProtoFileConverter;
            _kDataLayerProjectToKProtoFileConverter = kDataLayerProjectToKProtoFileConverter;
            _dataLayerServiceFactory = dataLayerServiceFactory;
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            _protoToKProtoConverter = protoToKProtoConverter;
        }
        public async Task<bool> Handle(BuildSolutionQuery query, CancellationToken cancellationToken)
        {    
            var kickstartWizardService = new  KickstartWizardService(_protoToKProtoConverter, _sProtoFileToProtoFileConverter, 
                _kDataLayerProjectToKProtoFileConverter, 
                _dbToKSolutionConverter,
                _dataLayerServiceFactory);

            var solution = kickstartWizardService.BuildSolution(query.KickstartModel);
            var solutionGroup = new KSolutionGroup();
            solutionGroup.Solution.Add(solution);
            var solutionGroupList = new List<KSolutionGroup>();
            solutionGroupList.Add(solutionGroup);

            _kickstartService.ProgressChanged += _kickstartService_ProgressChanged;
            await _kickstartService.ExecuteAsync(query.KickstartModel.ProjectDirectory, "bob", solutionGroupList);

            return true;
        }

        private void _kickstartService_ProgressChanged(object sender, KickstartProgressChangedEventArgs e)
        {
            
        }
    }
}
