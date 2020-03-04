using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Kickstart.Services.Types;
using Microsoft.AspNetCore.Mvc;
using static Kickstart.Services.Types.KickstartServiceApi;
using System.IO;

namespace Kickstart.Web.Controllers
{
    [Route("api/[controller]")]

    public class KickstartWizardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost("[action]")]
        public GeneratedSolution BuildSolution([FromBody] KickstartSolutionRequest generationModel)
        {
            generationModel.GenerateGrpcUnitTestProject = true;

            var channel = new Channel("localhost:50083", ChannelCredentials.Insecure);

            var client = new KickstartServiceApiClient(channel);

            var response = client.KickstartSolution(generationModel);

            return new GeneratedSolution
            {
                Succeeded = response.Succeeded,
                ErrorMessage = response.ErrorMessage,
                ZipAsBase64 = response.GeneratedFilesBase64
            };
            
        }
    }
    

    public class GeneratedSolution
    {
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; }
        public string ZipAsBase64 { get; set; }
    }
}