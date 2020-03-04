using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Kickstart.Services.Types;
using Microsoft.AspNetCore.Mvc;
using static Kickstart.Services.Types.KickstartServiceApi;

namespace Kickstart.Web.Controllers
{
    [Route("api/[controller]")]

    public class FlywaySplitterController : Controller
    {
        [HttpPost("[action]")]
        public Split Split([FromBody] SplitOptions splitOptions)
        {
            var channel = new Channel("localhost:50083", ChannelCredentials.Insecure);

            var client = new KickstartServiceApiClient(channel);
            
            var response = client.SplitDDL(
                new Services.Types.SplitDDLRequest
                {
                    DatabaseType = (DatabaseTypes)Enum.Parse(typeof(DatabaseTypes), splitOptions.DatabaseType),
                    
                    UnSplitTableDDL = splitOptions.UnSplitTableDDL,
                    UnSplitTableTypeDDL = splitOptions.UnSplitTableTypeDDL,
                    UnSplitViewDDL =  splitOptions.UnSplitViewDDL,
                    UnSplitFunctionDDL =  splitOptions.UnSplitFunctionDDL,
                    UnSplitStoredProcedureDDL = splitOptions.UnSplitStoredProcedureDDL
                });

            return  new Split()
            {
                ZipAsBase64 = response.ZipAsBase64
            };
        }
    }

    public class SplitOptions
    {
        public string UnSplitTableDDL { get; set; }
        public string UnSplitTableTypeDDL { get; set; }
        public string UnSplitViewDDL { get; set; }
        public string UnSplitFunctionDDL { get; set; }

        public string UnSplitStoredProcedureDDL { get; set; }
        public string DatabaseType { get; set; }
    }

    public class Split
    {
        public string ZipAsBase64 { get; set; }
    }
}