using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using System;
using Kickstart.Build.Data.Providers;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;

using Model = Kickstart.Build.Services.Model;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts.Conditions;

namespace Kickstart.Build.Services.Query
{
    public class CreateReleaseDefinitionHandler : IRequestHandler<CreateReleaseDefinitionQuery, Model.ReleaseDefinition>
    {
        private readonly ITfsProvider _tfsProvider;

        private static readonly Guid TaskDocker = Guid.Parse("e28912f1-0114-4464-802a-a3a35437fd16");

        public CreateReleaseDefinitionHandler(ITfsProvider tfsProvider)
        {
            _tfsProvider = tfsProvider;

        }

        public async Task<Model.ReleaseDefinition> Handle(
            CreateReleaseDefinitionQuery message,
            CancellationToken cancellationToken)
        {
            string buildDefinitionName = message.ReleaseDefinition.BuildDefinitions.Single().BuildDefinitionName;

            var connection = _tfsProvider.GetConnection() as VssConnection;

            var releaseServer = connection.GetClient<ReleaseHttpClient>(); // connect to the build server subpart
            var buildServer = connection.GetClient<BuildHttpClient>(); // connect to the build server subpart
            /*
            var releaseDefinitions = await releaseServer.GetReleaseDefinitionsAsync(project: message.ReleaseDefinition.ProjectId, searchText: "Feature");
            foreach (var rd in releaseDefinitions)
            {
                Console.WriteLine($"{rd.Name}");
                var rdFull = await releaseServer.GetReleaseDefinitionAsync(project: message.ReleaseDefinition.ProjectId, definitionId: rd.Id);
                var json = JsonConvert.SerializeObject(rdFull, Formatting.Indented);

            }
            */
            var releaseDefinition = new ReleaseDefinition()
            {
                Name = message.ReleaseDefinition.ReleaseDefinitionName,
                Variables = new Dictionary<string, ConfigurationVariableValue>
                {
                },
                VariableGroups = new List<int>
                {
                },
                Artifacts = new List<Artifact>()
                {
                },
                Environments = new List<ReleaseDefinitionEnvironment>()
                {
                }

            };

            AddEnvironmentVariableGroups(message, releaseDefinition.VariableGroups);

            AddReleaseVariables(message, message.ServiceName, releaseDefinition.Variables);


            int rank = 1;
            foreach (var environment in message.ReleaseDefinition.Environments)
            {
                var newEnv = BuildEnvironment(message, environment, buildDefinitionName);
                newEnv.Rank = rank;
                releaseDefinition.Environments.Add(newEnv);
                
                rank++;
            }

            foreach (var buildDefinition in message.ReleaseDefinition.BuildDefinitions)
            {
                if (buildDefinition.BuildDefinitionIdentifier == 0)
                {
                    //fill it in
                    var builddDefs = await buildServer.GetDefinitionsAsync2(name: buildDefinition.BuildDefinitionName, project: buildDefinition.ProjectId);
                    buildDefinition.BuildDefinitionIdentifier = builddDefs.Single().Id;

                }
                var artifact = BuildArtifact(message, buildDefinition);
                releaseDefinition.Artifacts.Add(artifact);
                //todo: conditionalize
                var trigger = new ArtifactSourceTrigger()
                {
                    ArtifactAlias = artifact.Alias,
                    TriggerConditions = new List<ArtifactFilter>
                    {
                    }
                };

                if (message.IsProdPath)
                {
                    trigger.TriggerConditions.Add( new ArtifactFilter() { SourceBranch = "release", UseBuildDefinitionBranch = false });
                }
                else
                {
                    trigger.TriggerConditions.Add(new ArtifactFilter() { SourceBranch = "dev", UseBuildDefinitionBranch = false });
                }

                releaseDefinition.Triggers.Add(trigger);

            }


            var createdReleaseDefinition = await releaseServer.CreateReleaseDefinitionAsync(releaseDefinition, project: message.ReleaseDefinition.ProjectId);

            return createdReleaseDefinition.ToModel();

        }

        private void AddEnvironmentVariableGroups(CreateReleaseDefinitionQuery message, IList<int> variableGroups)
        {
            //todo: make conditional
            //if (!message.IsProdPath) //need for Alpha, runs on Quark, but is Prod Path
            {
                variableGroups.Add(6);//DCOS-QA-Authorization
            }
            variableGroups.Add(16);//DCOSBuildVersion-Global
            variableGroups.Add(22);//stable container versions

            if (!message.IsProdPath)
            {
                variableGroups.Add(25);//AWS Dev Auth
            }
            //if (!message.IsProdPath) //need for Alpha ???
            {
                variableGroups.Add(18);//AWS QA Auth
            }
            //   
            if (message.IsProdPath)
            {
                variableGroups.Add(45);//DCOS-PROD-Authorization
                variableGroups.Add(28);//AWS UAT Auth

                variableGroups.Add(44);//AWS-Prod-Authorization
            }
        }

        private ReleaseDefinitionEnvironment BuildEnvironment(CreateReleaseDefinitionQuery message,  Model.Environment environment, string buildDefinitionName)
        {
            List<WorkflowTask> workflowTasks = GetWorkflowTasks(message, environment);

            var releaseDefinitionEnvironment = new ReleaseDefinitionEnvironment()
            {
                Name = environment.EnvironmentName,
                Variables = new Dictionary<string, ConfigurationVariableValue>(),
                RetentionPolicy = new EnvironmentRetentionPolicy
                {
                    DaysToKeep = 30,
                    ReleasesToKeep = 1
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
                                },
                                WorkflowTasks = workflowTasks
                            }
                        },
                Conditions = new List<Condition>()
                {
                    new Condition("ReleaseStarted", ConditionType.Event, string.Empty)
                }

            };

            AddEnvironmentVariables(message, environment, buildDefinitionName, releaseDefinitionEnvironment.Variables);

            return releaseDefinitionEnvironment;


        }

        private List<WorkflowTask> GetWorkflowTasks(CreateReleaseDefinitionQuery message, Model.Environment environment)
        {
            var workflowTasks = new List<WorkflowTask>();
            
            if (message.NeedsAWSAurora)
            {
                if (message.NeedsAwsUserAccountCreated)
                {
                    workflowTasks.Add(BuildWorkflowTaskCreateUserStack(message, environment));

                }
                workflowTasks.Add(BuildWorkflowTaskReuseDatabaseStack(message, environment));
                workflowTasks.Add(BuildWorkflowTaskCreateDatabase(message, environment));
                workflowTasks.Add(BuildWorkflowTaskCreateDatabaseUser(message, environment));
                workflowTasks.Add(BuildWorkflowTaskRunFlywayMigrations(message, environment));
            }
            if (message.NeedsDCOS)
            {
                workflowTasks.Add(BuildWorkflowTaskDcosDeploy(message, environment));
            }

            return workflowTasks;
        }
        
        private void AddEnvironmentVariables(CreateReleaseDefinitionQuery message, Model.Environment environment, string buildDefinitionName, IDictionary<string, ConfigurationVariableValue> variables)
        {
            var tag = environment.tag;
            var TAG = environment.TAG;

            if (message.NeedsDCOS)
            {
                var dcosServer = GetDcosServer(environment);
                variables.Add("DCOS_URL", new ConfigurationVariableValue { Value = $"https://dcos.{dcosServer}.company.io/" });
                variables.Add("DCOS_USERNAME", new ConfigurationVariableValue { Value = "tfs" });
            }

            variables.Add("AppName", new ConfigurationVariableValue { Value = $"/{message.ServiceName.ToLower()}/{tag}/{message.ServiceName.ToLower()}" });
            variables.Add("ArtifactAlias", new ConfigurationVariableValue { Value = $"$(System.ArtifactsDirectory)/{buildDefinitionName}/drop" });

            variables.Add("EnvironmentTag", new ConfigurationVariableValue { Value = TAG }); 

            //todo: Variable Groups are using Neutron for Dev, but on DCOS, its using $Host, so it queries Quark, I think. I had the wrong port so that may not be what was happening
            variables.Add("ConfigSvcTarget", new ConfigurationVariableValue { Value = $"$({TAG}_ConfigSvcTarget)" });

            if (message.NeedsAWS)
            {
                var altEnv = environment;
                if (environment.EnvironmentTag == Model.EnvironmentTag.Alpha)
                {
                    //there isn't a ALPHA version, so reuse QA
                    //todo: see if Vince can create ALPHA_ACCESS_KEY_ID
                    altEnv = new Model.Environment { EnvironmentTag = Model.EnvironmentTag.QA };
                }
                var awsAccessKeyId =  $"$({altEnv.TAG}_AWS_ACCESS_KEY_ID)";
                variables.Add("AWS_ACCESS_KEY_ID", new ConfigurationVariableValue { Value =  awsAccessKeyId});

                var awsASecretAccessKey = $"$({altEnv.TAG}_AWS_SECRET_ACCESS_KEY)";
                variables.Add("AWS_SECRET_ACCESS_KEY", new ConfigurationVariableValue { Value = awsASecretAccessKey });

                if ((message.IsProdPath))
                {
                    variables.Add("DB_STACK_NAME", new ConfigurationVariableValue { Value = $"{message.ReuseAwsStackFromServiceName.ToLower()}-{TAG}" });
                }
                else
                {
                    //atleast for feature flag stacks, dev/qa were created as lower case
                    //todo: get them constent
                    variables.Add("DB_STACK_NAME", new ConfigurationVariableValue { Value = $"{message.ReuseAwsStackFromServiceName.ToLower()}-{tag}" });

                }
                if (message.NeedsAwsUserAccountCreated)
                {
                    variables.Add("USER_STACK_NAME", new ConfigurationVariableValue { Value = $"{message.ServiceName}userstack" });

                    variables.Add("USER_TEMPLATE_PATH", new ConfigurationVariableValue { Value = $"$(System.ArtifactsDirectory)/{buildDefinitionName}/drop" });
                    
                }
            }

        }

        private void AddReleaseVariables(CreateReleaseDefinitionQuery message, string serviceName, IDictionary<string, ConfigurationVariableValue> variables)
        {

        
            if (message.NeedsAWS) 
            {
                variables.Add("ConfigSvcKey", new ConfigurationVariableValue { Value =  $"{ serviceName}" });

                //used in deploy.py, to tell it where to read params from when deploying Cloud Formation Template Stack
                variables.Add("CONFIG_SOURCES", new ConfigurationVariableValue { Value = "env,cfg_ctr" });

                variables.Add("DB_STACK_NAME", new ConfigurationVariableValue { Value = $"{message.ReuseAwsStackFromServiceName}-$(Release.EnvironmentName)" });
                if (message.NeedsAWSAurora) //AWS Aurora
                {
                    variables.Add("DB_PASSWORD_CONFIG_SVC_KEY", new ConfigurationVariableValue { Value = "CloudformationOutputs/Password" });
                    var dbName = message.DatabaseServers.Single().Databases.Single().DatabaseName;
                    if (dbName != dbName.ToLower() && message.DatabaseServers.Single().DbmsType == Model.DbmsType.Postgresql)
                    {
                        throw new InvalidOperationException("database names must be lowercase for Postgres");
                    }
                    variables.Add("DbName", new ConfigurationVariableValue { Value = dbName });//todo: parameterize  //things will blow up if db name not lowercase


                    variables.Add("VAULT_ADDR", new ConfigurationVariableValue { Value = "" });


                }
                if (message.NeedsAWS)
                {
                    //Aws Tags
                    variables.Add("ApplicationNameTag", new ConfigurationVariableValue { Value = message.ServiceName });
                    variables.Add("DepartmentTag", new ConfigurationVariableValue { Value = "Dept1" }); //todo
                    variables.Add("ResponsiblePartyTag", new ConfigurationVariableValue { Value = "Leader Name" }); //todo:
                    variables.Add("OwnerTag", new ConfigurationVariableValue { Value = "Leader Name" }); //todo
                }
            }
           
        }

        private string GetDcosServer(Model.Environment environment)
        {
            switch (environment.EnvironmentTag)
            {
                case Model.EnvironmentTag.Uat:
                case Model.EnvironmentTag.Prod:
                    return "proton";
                case Model.EnvironmentTag.Demo:
                case Model.EnvironmentTag.Alpha:
                case Model.EnvironmentTag.QA:
                    return "quark";
                case Model.EnvironmentTag.Dev:
                    return  "quark"; //deploy to Quark, read/write AWS param store/config center values to Neutron (AWS DEV)
            }

            throw new NotImplementedException();
        }

        private WorkflowTask BuildWorkflowTaskCreateUserStack(CreateReleaseDefinitionQuery message, Model.Environment environment)
        {
            return new WorkflowTask
            {
                TaskId = TaskDocker,
                Name = "Create User stack",
                RefName = "Docker_99",
                Version = "0.*",
                Enabled = true,
                AlwaysRun = false,
                ContinueOnError = false,
                TimeoutInMinutes = 0,
                DefinitionType = "task",
                Inputs = new Dictionary<string, string>
                                        {
                                            {"containerregistrytype","Container Registry" },
                                            {"dockerRegistryEndpoint","8dbcb6db-30c5-4c51-9c69-ace9c7b8913d" },
                                            {"action","Run an image" },
                                            {"dockerFile","**/Dockerfile" },
                                            {"buildArguments","" },
                                            {"defaultContext","true" },
                                            {"context","" },
                                            {"imageName","$(CFN-IMAGE-NAME)" },
                                            {"qualifyImageName","true" },
                                            {"additionalImageTags","" },
                                            {"includeSourceTags","false" },
                                            {"includeLatestTag","false" },
                                            {"imageDigestFile","" },
                                            {"containerName","" },
                                            {"ports","" },
                                            {"volumes","$(USER_TEMPLATE_PATH):/user.template" },
                                            {"envVars","ApplicationName=$(ApplicationNameTag)\nEnvironment=$(EnvironmentTag)\nDepartment=$(DepartmentTag)\nResponsibleParty=$(ResponsiblePartyTag)\nOwner=$(OwnerTag)\nCONFIG_KEY=$(ConfigSvcKey)\nCONFIG_SVC_TARGET=$(ConfigSvcTarget)\nAWS_ACCESS_KEY_ID=$(AWS_ACCESS_KEY_ID)\nAWS_SECRET_ACCESS_KEY=$(AWS_SECRET_ACCESS_KEY)\nAWS_DEFAULT_REGION=us-east-1\nDBSubnetGroup=$(DBSubnetGroup)" },
                                            {"workDir","" },
                                            {"entrypoint","" },
                                            {"containerCommand","python deploy.py /user.template/create-user.yml $(CONFIG_SOURCES) $(USER_STACK_NAME)-$(Release.EnvironmentName)" },
                                            {"detached","false" },
                                            {"restartPolicy","no" },
                                            {"restartMaxRetries","" },
                                            {"customCommand","" },
                                            {"dockerHostEndpoint","" },
                                            {"cwd","$(System.DefaultWorkingDirectory)" }

                }
            };
        }

        private WorkflowTask BuildWorkflowTaskReuseDatabaseStack(CreateReleaseDefinitionQuery message, Model.Environment environment)
        {
            return new WorkflowTask
            {
                TaskId = TaskDocker,
                Name = "Reuse database stack",
                RefName = "Docker_1",
                Version = "0.*",
                Enabled = true,
                AlwaysRun = false,
                ContinueOnError = false,
                TimeoutInMinutes = 0,
                DefinitionType = "task",
                Inputs = new Dictionary<string, string>
                                        {
                                            {"containerregistrytype","Container Registry" },
                                            {"dockerRegistryEndpoint","8dbcb6db-30c5-4c51-9c69-ace9c7b8913d" },
                                            {"action","Run an image" },
                                            {"dockerFile","**/Dockerfile" },
                                            {"buildArguments","" },
                                            {"defaultContext","true" },
                                            {"context","" },
                                            {"imageName","$(CFN-IMAGE-NAME)" },
                                            {"qualifyImageName","true" },
                                            {"additionalImageTags","" },
                                            {"includeSourceTags","false" },
                                            {"includeLatestTag","false" },
                                            {"imageDigestFile","" },
                                            {"containerName","" },
                                            {"ports","" },
                                            {"volumes","" },
                                            {"envVars","CONFIG_KEY=$(ConfigSvcKey)\nCONFIG_SVC_TARGET=$(ConfigSvcTarget)\nAWS_ACCESS_KEY_ID=$(AWS_ACCESS_KEY_ID)\nAWS_SECRET_ACCESS_KEY=$(AWS_SECRET_ACCESS_KEY)\nAWS_DEFAULT_REGION=us-east-1" },
                                            {"workDir","" },
                                            {"entrypoint","" },
                                            {"containerCommand","python output.py $(DB_STACK_NAME)" },
                                            {"detached","false" },
                                            {"restartPolicy","no" },
                                            {"restartMaxRetries","" },
                                            {"customCommand","" },
                                            {"dockerHostEndpoint","" },
                                            {"cwd","$(System.DefaultWorkingDirectory)" }

                }
            };
        }

        private WorkflowTask BuildWorkflowTaskCreateDatabase(CreateReleaseDefinitionQuery message, Model.Environment environment)
        {
            return new WorkflowTask
            {
                TaskId = TaskDocker,
                Name = "Create Database",
                RefName = "Docker_2",
                Version = "0.*",
                Enabled = true,
                AlwaysRun = false,
                ContinueOnError = false,
                TimeoutInMinutes = 0,
                DefinitionType = "task",
                Inputs = new Dictionary<string, string>
                                        {
                                            {"containerregistrytype","Container Registry" },
                                            {"dockerRegistryEndpoint","8dbcb6db-30c5-4c51-9c69-ace9c7b8913d" },
                                            {"action","Run an image" },
                                            {"dockerFile","**/Dockerfile" },
                                            {"buildArguments","" },
                                            {"defaultContext","true" },
                                            {"context","" },
                                            {"imageName","$(FLYWAY-IMAGE-NAME)" },
                                            {"qualifyImageName","true" },
                                            {"additionalImageTags","" },
                                            {"includeSourceTags","false" },
                                            {"includeLatestTag","false" },
                                            {"imageDigestFile","" },
                                            {"containerName","" },
                                            {"ports","" },
                                            {"volumes","" },
                                            {"envVars","CONFIG_SVC_TARGET=$(ConfigSvcTarget)\nCONFIG_KEY=$(ConfigSvcKey)\nDB_SERVER=localhost\nDB_USER=matrix\nDB_PASSWORD=$(DbPassword)\nDB_NAME=$(DbName)\nDB_DRIVER=postgresql\nDB_CMD=createdb\nDB_SERVER_CONFIG_SVC_KEY=CloudformationOutputs/DBEndpoint\nDB_PASSWORD_CONFIG_SVC_KEY=$(DB_PASSWORD_CONFIG_SVC_KEY)" },
                                            {"workDir","" },
                                            {"entrypoint","" },
                                            {"containerCommand","python run-flyway.py" },
                                            {"detached","false" },
                                            {"restartPolicy","no" },
                                            {"restartMaxRetries","" },
                                            {"customCommand","" },
                                            {"dockerHostEndpoint","" },
                                            {"cwd","$(System.DefaultWorkingDirectory)" }

                }
            };
        }

        private WorkflowTask BuildWorkflowTaskCreateDatabaseUser(CreateReleaseDefinitionQuery message, Model.Environment environment)
        {
            return new WorkflowTask
            {
                TaskId = TaskDocker,
                Name = "Create User",
                RefName = "Docker_3",
                Version = "0.*",
                Enabled = true,
                AlwaysRun = false,
                ContinueOnError = false,
                TimeoutInMinutes = 0,
                DefinitionType = "task",
                Inputs = new Dictionary<string, string>
                                        {
                                            { "containerregistrytype","Container Registry" },
                                            {"dockerRegistryEndpoint","8dbcb6db-30c5-4c51-9c69-ace9c7b8913d" },
                                            {"action","Run an image" },
                                            {"dockerFile","**/Dockerfile" },
                                            {"buildArguments","" },
                                            {"defaultContext","true" },
                                            {"context","" },
                                            {"imageName","$(FLYWAY-IMAGE-NAME)" },
                                            {"qualifyImageName","true" },
                                            {"additionalImageTags","" },
                                            {"includeSourceTags","false" },
                                            {"includeLatestTag","false" },
                                            {"imageDigestFile","" },
                                            {"containerName","" },
                                            {"ports","" },
                                            {"volumes","" },
                                            {"envVars","CONFIG_SVC_TARGET=$(ConfigSvcTarget)\nCONFIG_KEY=$(ConfigSvcKey)\nDB_SERVER=localhost\nDB_USER=matrix\nDB_PASSWORD=$(DbPassword)\nDB_NAME=$(DbName)\nDB_DRIVER=postgresql\nDB_CMD=create_user\nDB_SERVER_CONFIG_SVC_KEY=CloudformationOutputs/DBEndpoint\nDB_PASSWORD_CONFIG_SVC_KEY=$(DB_PASSWORD_CONFIG_SVC_KEY)\nNEW_DB_USER_CONFIG_SVC_KEY=CloudformationOutputs/DBUsername\nNEW_DB_PASSWORD_CONFIG_SVC_KEY=CloudformationOutputs/DBPassword" },
                                            {"workDir","" },
                                            {"entrypoint","" },
                                            {"containerCommand","python run-flyway.py" },
                                            {"detached","false" },
                                            {"restartPolicy","no" },
                                            {"restartMaxRetries","" },
                                            {"customCommand","" },
                                            {"dockerHostEndpoint","" },
                                            {"cwd","$(System.DefaultWorkingDirectory)" }


                }
            };
        }

        private WorkflowTask BuildWorkflowTaskRunFlywayMigrations(CreateReleaseDefinitionQuery message, Model.Environment environment)
        {
            return new WorkflowTask
            {
                TaskId = TaskDocker,
                Name = "Run Flyway Migrations",
                RefName = "Docker_4",
                Version = "0.*",
                Enabled = true,
                AlwaysRun = false,
                ContinueOnError = false,
                TimeoutInMinutes = 0,
                DefinitionType = "task",
                Inputs = new Dictionary<string, string>
                                        {
                                           { "containerregistrytype","Container Registry" },
                                            {"dockerRegistryEndpoint","8dbcb6db-30c5-4c51-9c69-ace9c7b8913d" },
                                            {"action","Run an image" },
                                            {"dockerFile","**/Dockerfile" },
                                            {"buildArguments","" },
                                            {"defaultContext","true" },
                                            {"context","" },
                                            {"imageName","$(FLYWAY-IMAGE-NAME)" },
                                            {"qualifyImageName","true" },
                                            {"additionalImageTags","" },
                                            {"includeSourceTags","false" },
                                            {"includeLatestTag","false" },
                                            {"imageDigestFile","" },
                                            {"containerName","" },
                                            {"ports","" },
                                            {"volumes",$"$(ArtifactAlias)/{message.DatabaseServers.Single().Databases.Single().ProjectFolder}/migrations:/migrations" },
                                            {"envVars","CONFIG_SVC_TARGET=$(ConfigSvcTarget)\nCONFIG_KEY=$(ConfigSvcKey)\nDB_SERVER=localhost\nDB_USER=matrix\nDB_PASSWORD=$(DbPassword)\nDB_NAME=$(DbName)\nDB_DRIVER=postgresql\nDB_CMD=migrate\nDB_SERVER_CONFIG_SVC_KEY=CloudformationOutputs/DBEndpoint\nDB_USER_CONFIG_SVC_KEY=CloudformationOutputs/DBUsername\nDB_PASSWORD_CONFIG_SVC_KEY=CloudformationOutputs/DBPassword" },
                                            {"workDir","" },
                                            {"entrypoint","" },
                                            {"containerCommand","python run-flyway.py" },
                                            {"detached","false" },
                                            {"restartPolicy","no" },
                                            {"restartMaxRetries","" },
                                            {"customCommand","" },
                                            {"dockerHostEndpoint","" },
                                            {"cwd","$(System.DefaultWorkingDirectory)" }
                }
            };
        }

        private WorkflowTask BuildWorkflowTaskDcosDeploy(CreateReleaseDefinitionQuery message, Model.Environment environment)
        {

            var task = new WorkflowTask
            {
                TaskId = Guid.Parse("5d5d4670-4126-417e-9b7f-ed5f99f9baef"),
                Name =  "Deploy service to DCOS", //"Task group: VO.TG.MarathonApp.DCOS.Deploy $(DCOSBuildVersion)",
                RefName = "VOTGMarathonAppDCOSDeploy_1",
                Version = "1.*",
                Enabled = true,
                AlwaysRun = true,
                ContinueOnError = false,
                TimeoutInMinutes = 0,
                DefinitionType = "metaTask",
                Inputs = new Dictionary<string, string>
                                        {
                                            {"DCOSBuildVersion","$(DCOSBuildVersion)" },
                                            {"ARTIFACT_PATH","$(ArtifactAlias)" },
                                            {"DCOS_URL","$(DCOS_URL)" },
                                            {"MARATHON_APP_DEF_ARTIFACT_PATH",$"{message.ServiceName.ToLower()}.json" },
                                            {"TFS_USER","" },
                                            {"TFS_PAT","$(TFS_PAT)" },
                                            {"APP_NAME","$(AppName)" },
                                            {"APP_INSTANCES","1" },
                                            {"WATCH_DEPLOY","" }

                                        }
            };
            /*
            if (message.IsProdPath)
            {
                task.Inputs.Add("DCOS_USERNAME", "");
                task.Inputs.Add("DCOS_PRIVATE_KEY_B64", "");
                task.Inputs.Add("CONFIG_REPO_BRANCH", "master");
                task.Inputs.Add("ENV_CONFIG_FILE", "");
            }
            else*/
            {
                task.Inputs.Add("DCOS_USERNAME", "$(DCOS_USERNAME)");
                switch (environment.EnvironmentTag)
                {
                    case Model.EnvironmentTag.Prod:
                        task.Inputs.Add("DCOS_PRIVATE_KEY_B64", "$(DCOS_PRIVATE_KEY_B64)");
                        break;
                    case Model.EnvironmentTag.Alpha:
                    case Model.EnvironmentTag.QA:
                        task.Inputs.Add("DCOS_PRIVATE_KEY_B64", "$(B64_KEY_QA)");
                        break;
                    case Model.EnvironmentTag.Dev:
                        task.Inputs.Add("DCOS_PRIVATE_KEY_B64", "$(B64_KEY_QA)");
                        break;
                }
                    
                
                if (message.IsProdPath)
                {
                    task.Inputs.Add("CONFIG_REPO_BRANCH", "master");

                    //todo: should Alpha use release?
                }
                else
                    task.Inputs.Add("CONFIG_REPO_BRANCH", "dev");

                var configurationFolder = message.ConfigurationFolder;
                if (message.IsProdPath)
                {
                    configurationFolder = $"/json/DCOS/{environment.TAG}";
                }
                task.Inputs.Add("ENV_CONFIG_FILE", $"{configurationFolder}/{message.ServiceName.ToLower()}.json");

            }

            return task;
        }

        private static Artifact BuildArtifact(CreateReleaseDefinitionQuery message, Model.BuildDefinition buildDefinition)
        {
            return new Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts.Artifact()
            {
                Type = "Build",
                Alias = buildDefinition.BuildDefinitionName,
                IsPrimary = true,
                IsRetained = false,
                DefinitionReference = new Dictionary<string, ArtifactSourceReference>
                        {
                            {
                                "definition",
                                new ArtifactSourceReference()
                                {
                                    Id = buildDefinition.BuildDefinitionIdentifier.ToString(),
                                    Name = buildDefinition.BuildDefinitionName
                                }
                            },
                            {
                                "project",
                                new ArtifactSourceReference()
                                {
                                    Id = message.ReleaseDefinition.ProjectId.ToString(),
                                    Name ="Company",
                                }
                            },
                            {
                                "defaultVersionType",
                                new ArtifactSourceReference()
                                {
                                    Id="latestType",
                                    Name ="Latest"
                                }
                            },
                            {
                                "defaultVersionBranch",
                                new ArtifactSourceReference()
                                {
                                    Id="",
                                    Name =""
                                }
                            },
                            {
                                "defaultVersionSpecific",
                                new ArtifactSourceReference()
                                {
                                    Id="",
                                    Name =""
                                }
                            },
                            {
                                "defaultVersionTags",
                                new ArtifactSourceReference()
                                {
                                    Id="",
                                    Name =""
                                }
                            },
                        }
            };
        }
    }

}
