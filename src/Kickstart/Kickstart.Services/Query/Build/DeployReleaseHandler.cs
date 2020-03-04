using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Kickstart.Build.Services.Model;
using System;
using Kickstart.Build.Data.Providers;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace Kickstart.Build.Services.Query
{
    public class DeployReleaseHandler : IRequestHandler<DeployReleaseQuery, bool>
    {
        private readonly ITfsProvider _tfsProvider;

        public DeployReleaseHandler(ITfsProvider tfsProvider)
        {
            _tfsProvider = tfsProvider;

        }

        public async Task<bool> Handle(DeployReleaseQuery message, CancellationToken cancellationToken)
        {
            var connection = _tfsProvider.GetConnection() as VssConnection;

            var releaseServer = connection.GetClient<ReleaseHttpClient>(); // connect to the build server subpart

            var release = await releaseServer.GetReleaseAsync(project: message.ProjectId, releaseId: message.ReleaseIdentifier);

            var releaseEnvironmentUpdateMetadata = new ReleaseEnvironmentUpdateMetadata()
            {
                Status = EnvironmentStatus.InProgress
            };

            int releaseEnvironmentId = release.Environments.First(e=>e.DefinitionEnvironmentId == message.EnvironmentIdentifier).Id; //  message.EnvironmentIdentifier;
            // Start deployment to an environment

            var releaseEnvironment = releaseServer.UpdateReleaseEnvironmentAsync(releaseEnvironmentUpdateMetadata, message.ProjectId, message.ReleaseIdentifier,
                releaseEnvironmentId).Result;

            return true;
        }

    }

}
