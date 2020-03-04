using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;

namespace Kickstart.Build2
{
    public class TfsReleaseService
    {
        private readonly ITfsConnectInfo _connectInfo;

        public TfsReleaseService ( ITfsConnectInfo connectInfo)
        {
            _connectInfo = connectInfo;
        }
        public async Task<Release> CreateReleaseAsync(ReleaseDefinition releaseDefinition, Build build)
        {
            var clientCredentials = new VssBasicCredential(string.Empty, _connectInfo.PAT);
            var connection = new VssConnection(_connectInfo.ServerUrl, clientCredentials);

            var releaseServer = connection.GetClient<ReleaseHttpClient>(); // connect to the build server subpart

            var releaseStartMetaData = new ReleaseStartMetadata()
            {
                DefinitionId = releaseDefinition.Id,
                IsDraft = false,
                
            };

            return await releaseServer.CreateReleaseAsync(releaseStartMetaData, project: _connectInfo.ProjectId);

        }
    }
}
