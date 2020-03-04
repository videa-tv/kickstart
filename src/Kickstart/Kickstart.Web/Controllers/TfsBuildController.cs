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
    public class TfsBuildController : Controller
    {

        [HttpPost("[action]")]
        public void GenerateBuild([FromBody] object buildFileJson)
        {  
            var request = CreateBuildDefinitionRequest.Parser.ParseJson(buildFileJson.ToString());
            var channel = new Channel("localhost:50095", ChannelCredentials.Insecure);

            var client = new KickstartBuildService.KickstartBuildServiceClient(channel);

            client.CreateBuildDefinition(request);
            
        }

    }
    
}
