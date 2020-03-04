//using Microsoft.VisualStudio.Services.Client;
//using Microsoft.TeamFoundation.SourceControl.WebApi;



using Kickstart.Build2;
using Microsoft.TeamFoundation.Build.WebApi;
using System;
using System.Collections.Generic;

namespace Kickstart.Build2
{
    class Program
    {
    
        static void Main(string[] args)
        {
            var connectInfo = new TfsConnectInfo();
            var buildService = new TfsBuildService(connectInfo);
            var buildDefintion = buildService.CreateBuildDefinition("Company", "partymodel", "PartyModel","PartyModelService", "DeleteMe").Result;
            var build = buildService.QueueBuildAsync(buildDefintion).Result;

            if (buildDefintion != null)
            {
                var releaseDefinitionService = new TfsReleaseDefinitionService(connectInfo);
                var releaseDefinition =  releaseDefinitionService.CreateReleaseDefinitionAsync("DeleteMe", new List<BuildDefinition> { buildDefintion }).Result;

                var releaseService = new TfsReleaseService(connectInfo);
                var release = releaseService.CreateReleaseAsync(releaseDefinition, build).Result;
            }
        }
    }
}
