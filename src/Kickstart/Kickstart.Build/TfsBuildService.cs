using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Net;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kickstart.Build2
{
    public class TfsBuildService
    {
        private static readonly Guid TaskShellScript = Guid.Parse("6c731c3c-3c68-459a-a5c9-bde6e6595b5b");
        private static readonly Guid TaskDocker = Guid.Parse("e28912f1-0114-4464-802a-a3a35437fd16");
        private static readonly Guid TaskPublishTestResults = Guid.Parse("0b0f01ed-7dde-43ff-9cbb-e48954daf9b1");
        private static readonly Guid TaskPublishBuildArtifacts = Guid.Parse("2ff763a7-ce83-4e1f-bc89-0ae63477cebe");
        private static readonly Guid DockerRegisteryEndpointId = Guid.Parse("8dbcb6db-30c5-4c51-9c69-ace9c7b8913d");

        private readonly ITfsConnectInfo _connectInfo;

        public TfsBuildService(ITfsConnectInfo connectInfo)
        {
            _connectInfo = connectInfo;
        }
        public async Task<BuildDefinition> CreateBuildDefinition(string projectName, string repoName, string repoPath, string serviceName, string buildName)
        {
            var clientCredentials = new VssBasicCredential(string.Empty, _connectInfo.PAT);
            var connection = new VssConnection(_connectInfo.ServerUrl, clientCredentials);
            
            
            var git = connection.GetClient<GitHttpClient>();
            var repos = git.GetRepositoriesAsync().Result;

            var theRepo = repos.First(r => r.Name.ToLower() == repoName);
            var sourceControlServer = connection.GetClient<TfvcHttpClient>(); // connect to the TFS source control subpart
            var buildServer = connection.GetClient<BuildHttpClient>(); // connect to the build server subpart
            
            var buildDefinition = new BuildDefinition()
            {
                Name = buildName,
                Project = new Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference
                {
                    Id = _connectInfo.ProjectId,
                    Name = projectName
                },
                Repository = new BuildRepository
                {
                    Id = theRepo.Id.ToString(),
                    Type = "TfsGit",
                    DefaultBranch = "refs/heads/dev"
                },
                Queue = new AgentPoolQueue
                {
                    Id = 7 //todo
                }
            };
            var process = new DesignerProcess()
            {

            };
            var phase1 = new Phase()
            {
                Name = "Phase 1"
            };
            if (!string.IsNullOrEmpty(repoPath))
            {
                repoPath += "/";
            }
            phase1.Steps.Add(BuildStepBuildApps(repoPath));
            phase1.Steps.Add(BuildStepTestApps(repoPath));
            phase1.Steps.Add(BuildStepPublishTestResults(repoPath));
            phase1.Steps.Add(BuildStepSetVars(repoPath));
            phase1.Steps.Add(BuildStepBuildServiceImage(repoPath));
            phase1.Steps.Add(BuildStepBuildOutput(repoPath, serviceName));
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
            

            return buildDefinitionResult;
        }
        public async Task<Build> QueueBuildAsync(BuildDefinition buildDefinition)
        {
            var clientCredentials = new VssBasicCredential(string.Empty, _connectInfo.PAT);
            var connection = new VssConnection(_connectInfo.ServerUrl, clientCredentials);
            var buildServer = connection.GetClient<BuildHttpClient>(); // connect to the build server subpart

            var build = new Build()
            {
                Definition = buildDefinition,
                Project = buildDefinition.Project
            };
            try
            {
                var queuedBuild = await buildServer.QueueBuildAsync(build);
                Console.WriteLine($"Queued build");
                do
                {
                    Console.WriteLine($"Build is in progress. Waiting 1 second.");
                    Thread.Sleep(1000);
                    queuedBuild = await buildServer.GetBuildAsync(queuedBuild.Id);
                }
                while (queuedBuild.Status != BuildStatus.Completed);
                Console.WriteLine("Build is complete");
                return queuedBuild;

            }
            finally
            {
                // buildServer.DeleteDefinitionAsync(definitionId: buildDefinitionResult.Id, project: buildDefinitionResult.Project.Id).Wait();
                // Console.WriteLine($"Deleted build");
            }
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
                    {"imageName","registry.videa.tv/dotnet-build:2.5.1" },
                    {"qualifyImageName","false" },
                    {"additionalImageTags","" },
                    {"includeSourceTags","false" },
                    {"includeLatestTag","false" },
                    {"imageDigestFile","" },
                    {"containerName","" },
                    {"ports","" },
                    {"volumes","$(Agent.BuildDirectory):/work" },
                    {"envVars","imageNameService=registry.videa.tv/$(serviceImageName)-$(Build.BuildId)" },
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
