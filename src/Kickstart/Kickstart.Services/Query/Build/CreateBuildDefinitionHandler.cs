using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using System;
using Kickstart.Build.Data.Providers;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;
using Model = Kickstart.Build.Services.Model;
namespace Kickstart.Build.Services.Query
{
    public class CreateBuildDefinitionHandler : IRequestHandler<CreateBuildDefinitionQuery, Model.BuildDefinition>
    {
        private readonly ITfsProvider _tfsProvider;

        private static readonly Guid TaskShellScript = Guid.Parse("6c731c3c-3c68-459a-a5c9-bde6e6595b5b");
        private static readonly Guid TaskDocker = Guid.Parse("e28912f1-0114-4464-802a-a3a35437fd16");
        private static readonly Guid TaskPublishTestResults = Guid.Parse("0b0f01ed-7dde-43ff-9cbb-e48954daf9b1");
        private static readonly Guid TaskPublishBuildArtifacts = Guid.Parse("2ff763a7-ce83-4e1f-bc89-0ae63477cebe");
        private static readonly Guid DockerRegisteryEndpointId = Guid.Parse("8dbcb6db-30c5-4c51-9c69-ace9c7b8913d");

        public CreateBuildDefinitionHandler(ITfsProvider tfsProvider)
        {
              _tfsProvider = tfsProvider;

        }

        public async Task<Model.BuildDefinition> Handle(
            CreateBuildDefinitionQuery message, 
            CancellationToken cancellationToken)
        {
            var connection = _tfsProvider.GetConnection() as VssConnection;
            
            var git = connection.GetClient<GitHttpClient>();
            var repos = git.GetRepositoriesAsync().Result;

            var theRepo = repos.First(r => r.Name.ToLower() == message.BuildDefinition.RepoName);
            var sourceControlServer = connection.GetClient<TfvcHttpClient>(); // connect to the TFS source control subpart
            var buildServer = connection.GetClient<BuildHttpClient>(); // connect to the build server subpart
            
            var buildDefinition = new BuildDefinition()
            {
                Name = message.BuildDefinition.BuildDefinitionName,
                Project = new Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference
                {
                    Id = message.BuildDefinition.ProjectId,
                    Name = message.BuildDefinition.ProjectName
                },
                Repository = new BuildRepository
                {
                    Id = theRepo.Id.ToString(),
                    Type = "TfsGit",
                    DefaultBranch = "refs/heads/dev" //todo: change to /release
                },
                Queue = new AgentPoolQueue
                {
                    Id = 7 //todo
                }
            };
            //Continuous integration
            var continuousIntegrationTrigger = new ContinuousIntegrationTrigger() { BatchChanges = false };
            continuousIntegrationTrigger.BranchFilters.Add("+refs/heads/dev");
            continuousIntegrationTrigger.BranchFilters.Add("+refs/heads/release");

            if (!string.IsNullOrEmpty(message.BuildDefinition.RepoPath))
            {
                continuousIntegrationTrigger.PathFilters.Add($"-/");
                continuousIntegrationTrigger.PathFilters.Add($"+/{message.BuildDefinition.RepoPath}");
            }

            buildDefinition.Triggers.Add(continuousIntegrationTrigger);
            //end Continuous Integration

            var process = new DesignerProcess()
            {

            };
            var phase1 = new Phase()
            {
                Name = "Phase 1"
            };
            string repoPath = message.BuildDefinition.RepoPath;

            if (!string.IsNullOrEmpty(message.BuildDefinition.RepoPath))
            {
                repoPath += "/";
            }
            phase1.Steps.Add(BuildStepBuildApps(repoPath));
            phase1.Steps.Add(BuildStepTestApps(repoPath));
            phase1.Steps.Add(BuildStepPublishTestResults(repoPath));
            phase1.Steps.Add(BuildStepSetVars(repoPath));
            phase1.Steps.Add(BuildStepBuildServiceImage(repoPath));
            phase1.Steps.Add(BuildStepBuildOutput(repoPath, message.ServiceName));
            phase1.Steps.Add(BuildStepPushServiceImage(repoPath));
            phase1.Steps.Add(BuildStepPublishArtifact());

            process.Phases.Add(phase1);
            buildDefinition.Process = process;

            BuildDefinition buildDefinitionResult = null;
            try
            {
                buildDefinitionResult = buildServer.CreateDefinitionAsync(buildDefinition).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            Console.WriteLine($"Created build definition {buildDefinition.Name} {buildDefinitionResult.Id}");

            return buildDefinitionResult.ToModel();
        }

        private BuildDefinitionStep BuildStepBuildApps(string repoPath)
        {
            return new BuildDefinitionStep
            {
                DisplayName = "build apps",
                Enabled = true,
                Inputs = new Dictionary<string, string>()
                {
                    {"scriptPath",  $"{repoPath}build/run-build-apps.sh" },
                    {"args", "\"$(Build.StagingDirectory)\""},
                    {"disableAutoCwd", "true" },
                    {"cwd", "" },
                    {"failOnStandardError","false" }
                },
                TaskDefinition = new TaskDefinitionReference
                {
                    Id = TaskShellScript,
                    VersionSpec = "2.*",
                    DefinitionType = "task"
                }
            };
        }

        private BuildDefinitionStep BuildStepTestApps(string repoPath)
        {
            return new BuildDefinitionStep
            {
                DisplayName = "test apps",
                Enabled = true,
                Inputs = new Dictionary<string, string>()
                {
                    {"scriptPath",  $"{repoPath}build/run-test-apps.sh" },
                    {"args", "\"$(Build.StagingDirectory)\""},
                    {"disableAutoCwd", "true" },
                    {"cwd", "" },
                    {"failOnStandardError","false" }
                },
                TaskDefinition = new TaskDefinitionReference
                {
                    Id = TaskShellScript,
                    VersionSpec = "2.*",
                    DefinitionType = "task"
                }
            };
        }

        private BuildDefinitionStep BuildStepPublishTestResults(string repoPath)
        {
            return new BuildDefinitionStep
            {
                DisplayName = "publish test results",
                Enabled = true,
                Inputs = new Dictionary<string, string>()
                {
                    {"testRunner","VSTest" },
                    {"testResultsFiles","../**/TestResults/*.trx" },
                    {"mergeTestResults","false" },
                    {"testRunTitle","" },
                    {"platform","" },
                    {"configuration","" },
                    {"publishRunAttachments","true" }

                },
                TaskDefinition = new TaskDefinitionReference
                {
                    Id = TaskPublishTestResults,
                    VersionSpec = "1.*",
                    DefinitionType = "task"
                }
            };
        }

        private BuildDefinitionStep BuildStepSetVars(string repoPath)
        {
            return new BuildDefinitionStep
            {
                DisplayName = "set vars",
                Enabled = true,
                Inputs = new Dictionary<string, string>()
                {
                    {"scriptPath",$"{repoPath}build/set-vars.sh" },
                    {"args","\"$(Build.StagingDirectory)\"" },
                    {"disableAutoCwd","false" },
                    {"cwd","" },
                    {"failOnStandardError","false" }
                },
                TaskDefinition = new TaskDefinitionReference
                {
                    Id = TaskShellScript,
                    VersionSpec = "2.*",
                    DefinitionType = "task"
                }
            };
        }

        private BuildDefinitionStep BuildStepPushServiceImage(string repoPath)
        {
            return new BuildDefinitionStep
            {
                DisplayName = "Push service image",
                Enabled = true,
                Inputs = new Dictionary<string, string>()
                {
                    {"containerregistrytype","Container Registry" },
                    {"dockerRegistryEndpoint",DockerRegisteryEndpointId.ToString() },
                    {"action","Push an image" },
                    {"dockerFile","$(serviceImageName)-$(Build.BuildId)" },
                    {"buildArguments","" },
                    {"defaultContext","true" },
                    {"context","" },
                    {"imageName","$(serviceImageName)-$(Build.BuildId)" },
                    {"qualifyImageName","true" },
                    {"additionalImageTags","" },
                    {"includeSourceTags","false" },
                    {"includeLatestTag","false" },
                    {"imageDigestFile","$(Build.ArtifactStagingDirectory)/drop/digest.txt" },
                    {"containerName","" },
                    {"ports","" },
                    {"volumes","" },
                    {"envVars","" },
                    {"workDir","" },
                    {"entrypoint","" },
                    {"containerCommand","" },
                    {"detached","true" },
                    {"restartPolicy","no" },
                    {"restartMaxRetries","" },
                    {"customCommand","" },
                    {"dockerHostEndpoint","" },
                    {"cwd","$(System.DefaultWorkingDirectory)" }
                },
                TaskDefinition = new TaskDefinitionReference
                {
                    Id = TaskDocker,
                    VersionSpec = "0.*",
                    DefinitionType = "task"
                }
            };
        }
        private BuildDefinitionStep BuildStepBuildServiceImage(string repoPath)
        {
            return new BuildDefinitionStep
            {
                DisplayName = "Build service image",
                Enabled = true,
                Inputs = new Dictionary<string, string>()
                {
                    {"containerregistrytype","Container Registry" },
                    {"dockerRegistryEndpoint",DockerRegisteryEndpointId.ToString() },
                    {"action","Build an image" },
                    {"dockerFile","$(Build.StagingDirectory)/Service/Dockerfile" },
                    {"buildArguments","GIT_COMMIT=$(Build.SourceVersion)\nBUILD_NUMBER=$(Build.BuildNumber)" },
                    {"defaultContext","true" },
                    {"context","" },
                    {"imageName","$(serviceImageName)-$(Build.BuildId)" },
                    {"qualifyImageName","true" },
                    {"additionalImageTags","" },
                    {"includeSourceTags","false" },
                    {"includeLatestTag","false" },
                    {"imageDigestFile","" },
                    {"containerName","" },
                    {"ports","" },
                    {"volumes","" },
                    {"envVars","" },
                    {"workDir","" },
                    {"entrypoint","" },
                    {"containerCommand","" },
                    {"detached","true" },
                    {"restartPolicy","no" },
                    {"restartMaxRetries","" },
                    {"customCommand","" },
                    {"dockerHostEndpoint","" },
                    {"cwd","$(System.DefaultWorkingDirectory)" }

                },
                TaskDefinition = new TaskDefinitionReference
                {
                    Id = TaskDocker,
                    VersionSpec = "0.*",
                    DefinitionType = "task"
                }
            };
        }

        private BuildDefinitionStep BuildStepBuildOutput(string repoPath, string serviceName)
        {
            return new BuildDefinitionStep
            {
                DisplayName = "Build output",
                Enabled = true,
                Inputs = new Dictionary<string, string>()
                {
                    {"containerregistrytype","Container Registry" },
                    {"dockerRegistryEndpoint",DockerRegisteryEndpointId.ToString() },
                    {"action","Run an image" },
                    {"dockerFile","**/Dockerfile" },
                    {"buildArguments","" },
                    {"defaultContext","true" },
                    {"context","" },
                    {"imageName","registry.company.com/dotnet-build:2.6.0" },
                    {"qualifyImageName","false" },
                    {"additionalImageTags","" },
                    {"includeSourceTags","false" },
                    {"includeLatestTag","false" },
                    {"imageDigestFile","" },
                    {"containerName","" },
                    {"ports","" },
                    {"volumes","$(Agent.BuildDirectory):/work" },
                    {"envVars","imageNameService=registry.company.com/$(serviceImageName)-$(Build.BuildId)" },
                    {"workDir","/work" },
                    {"entrypoint","" },
                    {"containerCommand",$"/bin/bash -c \"test -d /work/a/drop || mkdir /work/a/drop && envsubst '$imageNameService' < /work/s/{repoPath}/build/{serviceName.ToLower()}.json > /work/a/drop/{serviceName.ToLower()}.json\"" },
                    {"detached","false" },
                    {"restartPolicy","no" },
                    {"restartMaxRetries","" },
                    {"customCommand","" },
                    {"dockerHostEndpoint","" },
                    {"cwd","$(System.DefaultWorkingDirectory)" }

                },
                TaskDefinition = new TaskDefinitionReference
                {
                    Id = TaskDocker,
                    VersionSpec = "0.*",
                    DefinitionType = "task"
                }
            };
        }


        private BuildDefinitionStep BuildStepPublishArtifact()
        {
            return new BuildDefinitionStep
            {
                DisplayName = "Publish Artifact: drop",
                Enabled = true,
                Inputs = new Dictionary<string, string>()
                {
                    {"PathtoPublish","$(Build.ArtifactStagingDirectory)/drop" },
                    {"ArtifactName","drop" },
                    {"ArtifactType","Container" },
                    {"TargetPath","\\\\my\\share\\$(Build.DefinitionName)\\$(Build.BuildNumber)" }
                },
                TaskDefinition = new TaskDefinitionReference
                {
                    Id = TaskPublishBuildArtifacts,
                    VersionSpec = "1.*",
                    DefinitionType = "task"
                }
            };
        }

    }

}
