using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using System;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Kickstart.Build.Data.Providers;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.TeamFoundation.Build.WebApi;

namespace Kickstart.Build.Services.Query
{
    public class CreateReleaseHandler : IRequestHandler<CreateReleaseQuery, bool>
    {
        private readonly ITfsProvider _tfsProvider;

        public CreateReleaseHandler(ITfsProvider tfsProvider)
        {
            _tfsProvider = tfsProvider;
        }

        public async Task<bool> Handle(CreateReleaseQuery message, CancellationToken cancellationToken)
        {
            var connection = _tfsProvider.GetConnection() as VssConnection;

            var releaseServer = connection.GetClient<ReleaseHttpClient>(); // connect to the build server subpart
            var buildServer = connection.GetClient<BuildHttpClient>(); // connect to the build server subpart

            if (message.ReleaseDefinition.ReleaseDefinitionIdentifier == 0)
            {
                //fill it in
                //todo: verify returns only single item
                var rd = await releaseServer.GetReleaseDefinitionsAsync(project: message.ReleaseDefinition.ProjectId, searchText: message.ReleaseDefinition.ReleaseDefinitionName);

                foreach (var rdi in rd)
                {
                    if (rdi.Name == message.ReleaseDefinition.ReleaseDefinitionName)
                    {
                        message.ReleaseDefinition.ReleaseDefinitionIdentifier = rdi.Id;
                        break;
                    }
                }
            }
            
            var releaseStartMetaData = new ReleaseStartMetadata()
            {
                DefinitionId = message.ReleaseDefinition.ReleaseDefinitionIdentifier,
                
                IsDraft = false,
                Artifacts = new List<ArtifactMetadata>()
                {
                    
                }

            };

            foreach (var buildDefinition in message.ReleaseDefinition.BuildDefinitions)
            {
                if (buildDefinition.BuildDefinitionIdentifier == 0)
                {
                    //fill it in
                    var builddDefs = await buildServer.GetDefinitionsAsync2(name: buildDefinition.BuildDefinitionName, project: buildDefinition.ProjectId);
                    buildDefinition.BuildDefinitionIdentifier = builddDefs.Single().Id;
                }

                var lastBuild = buildServer.GetBuildsAsync(project: message.ReleaseDefinition.ProjectId, definitions : new[] { buildDefinition.BuildDefinitionIdentifier }, statusFilter: BuildStatus.Completed).Result
                        .OrderByDescending(b => b.Id)
                        .FirstOrDefault();
                releaseStartMetaData.Artifacts.Add(
                new ArtifactMetadata
                {
                    Alias = buildDefinition.BuildDefinitionName,
                    InstanceReference = new BuildVersion
                    {
                        Id = lastBuild.Id.ToString(),
                        Name = lastBuild.BuildNumber
                    }
                });

            }
            var result = await releaseServer.CreateReleaseAsync(releaseStartMetaData, project: message.ReleaseDefinition.ProjectId);

            return true;
        }

    }

}
