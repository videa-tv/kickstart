using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Kickstart.Build.Services.Model;
using System;
using Kickstart.Build.Data.Providers;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.Extensions.Logging;

namespace Kickstart.Build.Services.Query
{
    public class QueueBuildHandler : IRequestHandler<QueueBuildQuery, Model.Build>
    {
        private readonly ITfsProvider _tfsProvider;
        private readonly ILogger<QueueBuildHandler> _logger;

        public QueueBuildHandler(ILogger<QueueBuildHandler> logger, ITfsProvider tfsProvider)
        {
            _logger = logger;
            _tfsProvider = tfsProvider;

        }

        public async Task<Model.Build> Handle(
            QueueBuildQuery message, 
            CancellationToken cancellationToken)
        {
            var connection = _tfsProvider.GetConnection() as VssConnection;

            var buildServer = connection.GetClient<BuildHttpClient>(); // connect to the build server subpart
            if (message.BuildDefinition.BuildDefinitionIdentifier == 0)
            {
                //fill it in
                var builddDefs = await buildServer.GetDefinitionsAsync2(name: message.BuildDefinition.BuildDefinitionName, project: message.BuildDefinition.ProjectId);
                message.BuildDefinition.BuildDefinitionIdentifier = builddDefs.Single().Id;
            }
            var build = new Microsoft.TeamFoundation.Build.WebApi.Build()
            {
                Definition = message.BuildDefinition.ToEntity(),
                Project = message.BuildDefinition.ToEntity().Project
            };
            try
            {
                var queuedBuild = await buildServer.QueueBuildAsync(build);
                _logger.LogInformation("Queued build for {id} {name}", build.Definition.Id, message.BuildDefinition.BuildDefinitionName);
                do
                {
                    _logger.LogInformation("Build {buildId} is {status}. Waiting 1 second.", queuedBuild.Id, queuedBuild.Status);
                    Thread.Sleep(1000);
                    queuedBuild = await buildServer.GetBuildAsync(message.BuildDefinition.ToEntity().Project.Id, queuedBuild.Id);
                }
                while (queuedBuild.Status != BuildStatus.Completed);
                _logger.LogInformation("Build is complete. Status {buildResult}", queuedBuild.Result);
                return queuedBuild.ToModel();

            }
            finally
            {
                // buildServer.DeleteDefinitionAsync(definitionId: buildDefinitionResult.Id, project: buildDefinitionResult.Project.Id).Wait();
                // Console.WriteLine($"Deleted build");
            }
        }

    }

}
