using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Kickstart.Utility
{
    /// <summary>
    /// make sure Goole Nugets have been installed
    /// </summary>
    public class PrereqChecker : IPrereqChecker
    {
        private readonly IConfigurationRoot _configuration;
        public PrereqChecker(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public bool CheckGrpcNugets()
        {
            var versionGrpcTools = _configuration.GetValue<string>("Grpc_Tools_NugetVersion");
            var versionGoogleProtobuf = _configuration.GetValue<string>("Google_Protobuf_NugetVersion");

            if (Directory.Exists($@"%USERPROFILE%\.nuget\packages\Grpc.Tools\{versionGrpcTools}\tools\windows_x64\"))
                return false;

            if (Directory.Exists($@"%USERPROFILE%\.nuget\packages\Google.Protobuf.Tools\{versionGoogleProtobuf}\tools"))
                return false;

            if (Directory.Exists($@"%USERPROFILE%\.nuget\packages\Grpc.Tools\{versionGrpcTools}\tools\windows_x64\"))
                return false;

            return true;
        }
    }
}
