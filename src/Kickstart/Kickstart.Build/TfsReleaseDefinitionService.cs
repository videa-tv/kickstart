using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Build2
{
    public class TfsReleaseDefinitionService
    {
        private readonly ITfsConnectInfo _connectInfo;
        public TfsReleaseDefinitionService(ITfsConnectInfo connectInfo)
        {
            _connectInfo = connectInfo;
        }
        public async Task<ReleaseDefinition> CreateReleaseDefinitionAsync(string releaseDefinitionName, IEnumerable<BuildDefinition> buildDefinitions)
        {
            
            var clientCredentials = new VssBasicCredential(string.Empty, _connectInfo.PAT);
            var connection = new VssConnection(_connectInfo.ServerUrl, clientCredentials);

            var releaseServer = connection.GetClient<ReleaseHttpClient>(); // connect to the build server subpart

            var releaseDefinitions = await releaseServer.GetReleaseDefinitionsAsync(project: _connectInfo.ProjectId, searchText: "Feature");
            foreach (var rd in releaseDefinitions)
            {
                Console.WriteLine($"{rd.Name}");
                var rdFull = await releaseServer.GetReleaseDefinitionAsync(project: _connectInfo.ProjectId, definitionId: rd.Id);
                var json = JsonConvert.SerializeObject(rdFull, Formatting.Indented);
                int x = 1;
                
            }
            var releaseDefintion = new ReleaseDefinition()
            {
                Name = releaseDefinitionName,
                Artifacts = new List<Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts.Artifact>()
                {
                    new Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts.Artifact()
                    {
                        Type="Build",
                        Alias =buildDefinitions.First().Name,
                        DefinitionReference = new Dictionary<string, ArtifactSourceReference>
                        {
                            {
                                "definition",
                                new ArtifactSourceReference()
                                {
                                    Id = buildDefinitions.First().Id.ToString(),
                                    Name = buildDefinitions.First().Name

                                }
                            },
                            {
                             "project",
                                new ArtifactSourceReference()
                                {
                                    Id = _connectInfo.ProjectId.ToString(),
                                    Name ="Company",
                                }
                            }
                        }
                    }
                },
                Environments = new List<ReleaseDefinitionEnvironment>()
                {
                    new ReleaseDefinitionEnvironment()
                    {
                        Name="Dev",
                        RetentionPolicy = new EnvironmentRetentionPolicy
                        {
                            DaysToKeep = 30,
                            ReleasesToKeep =1
                        },
                        PreDeployApprovals = new ReleaseDefinitionApprovals()
                        {
                            Approvals = new List<ReleaseDefinitionApprovalStep>()
                            {
                                
                                new ReleaseDefinitionApprovalStep()
                                {
                                    Rank= 1,
                                    IsAutomated = true
                                }
                            }
                        },
                        PostDeployApprovals = new ReleaseDefinitionApprovals()
                        {
                            Approvals = new List<ReleaseDefinitionApprovalStep>
                            {
                                new ReleaseDefinitionApprovalStep()
                                {
                                    Rank= 1,
                                    IsAutomated = true
                                }
                            }
                        },
                        DeployPhases = new List<DeployPhase>()
                        {
                            new AgentBasedDeployPhase()
                            {
                                Name="Run on agent",
                                Rank =1,
                                DeploymentInput = new AgentDeploymentInput()
                                {

                                    QueueId = 7 //linux //todo
                                }
                            }
                        }

                    }
                }

            };

            var createdReleaseDefinition = await releaseServer.CreateReleaseDefinitionAsync(releaseDefintion, project: _connectInfo.ProjectId);

            return createdReleaseDefinition;
            
        }
    }
}
