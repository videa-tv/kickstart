using Microsoft.Extensions.Configuration;

namespace Kickstart.Build.Services.Config
{
    public class TracingOptions
    {
        public bool Enabled { get; set; }
        public bool Verbose { get; set; }
        public string ServiceName { get; set; }
        public string TracingAddress { get; set; }

        public TracingOptions(IConfiguration configuration)
        {
            configuration.GetSection("Tracing").Bind(this);

        }


    }

}
