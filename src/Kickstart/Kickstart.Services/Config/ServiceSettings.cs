using System;
using System.Collections.Generic;

namespace Kickstart.Services.Config
{
    public interface IServiceSettings
    {
        ApiHosts ApiHosts { get; set; }
        int Port { get; set; }
    }
    public class ServiceSettings : IServiceSettings
     {
        public ApiHosts ApiHosts { get; set; } 
        public int Port { get; set; } 
        public LimitsSettings Limits { get; set; } 
    }
}
