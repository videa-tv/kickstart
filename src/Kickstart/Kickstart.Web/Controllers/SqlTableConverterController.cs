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

    public class SqlServerConverterController : Controller
    {
        [HttpPost("[action]")]
        public Converted Convert([FromBody] ConvertOptions convertOptions)
        {
            var channel = new Channel("localhost:50083", ChannelCredentials.Insecure);

            var client = new KickstartServiceApiClient(channel);
            
            var response = client.ConvertDDL(
                new Services.Types.ConvertDDLRequest
                {
                    DatabaseType = (DatabaseTypes)Enum.Parse(typeof(DatabaseTypes), convertOptions.ConvertToDatabaseType),
                    ConvertToSnakeCase = convertOptions.ConvertToSnakeCase,
                    
                    UnconvertedTableDDL = convertOptions.UnconvertedTableDDL,
                    UnconvertedTableTypeDDL = convertOptions.UnconvertedTableTypeDDL,
                    UnconvertedStoredProcedureDDL = convertOptions.UnconvertedStoredProcedureDDL
                });

            return  new Converted()
            {
                ConvertedTableDDL = response.ConvertedTableDDL,
                ConvertedTableTypeDDL = response.ConvertedTableTypeDDL,
                ConvertedStoredProcedureDDL = response.ConvertedStoredProcedureDDL,
                ConvertedDmsJson = response.ConvertedDmsJson,
                ZipAsBase64 = response.ZipAsBase64
            };
        }
    }

    public class ConvertOptions
    {
        public bool ConvertToSnakeCase { get; set; }
        public string UnconvertedTableDDL { get; set; }
        public string UnconvertedTableTypeDDL { get; set; }
        public string UnconvertedStoredProcedureDDL { get; set; }
        public string ConvertToDatabaseType { get; set; }
    }

    public class Converted
    {
        public string ConvertedTableDDL { get; set; }

        public string ConvertedTableTypeDDL { get; set; }

        public string ConvertedStoredProcedureDDL { get; set; }

        public string ConvertedDmsJson { get; set; }
        public string ZipAsBase64 { get; set; }
    }
}