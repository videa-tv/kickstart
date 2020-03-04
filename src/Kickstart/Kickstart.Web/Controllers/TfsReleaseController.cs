using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Kickstart.Services.Types;
using Microsoft.AspNetCore.Mvc;
using Company.KickstartBuild.Services.Types;

namespace Kickstart.Web.Controllers
{
    [Route("api/[controller]")]
    public class TfsReleaseController : Controller
    {

        [HttpPost("[action]")]
        public void GenerateRelease([FromBody] object releaseDefinitionJson)
        {  
            var request = CreateReleaseDefinitionRequest.Parser.ParseJson(releaseDefinitionJson.ToString());
            var channel = new Channel("localhost:50095", ChannelCredentials.Insecure);

            var client = new KickstartBuildService.KickstartBuildServiceClient(channel);

            client.CreateReleaseDefinition(request);
            
        }

    }
    
}
