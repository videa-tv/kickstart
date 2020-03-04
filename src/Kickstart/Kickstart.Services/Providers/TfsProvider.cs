
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;

namespace Kickstart.Build.Data.Providers
{
    public class TfsProvider : ITfsProvider
    {
        public Uri ServerUrl { get; set; } = new Uri("https://tfs.company.com/tfs/Company/");
        public string PAT { get; set; } = "344apnq33nqwk2ntvzfhmke73dz7kn7nx4gdin7a5jeqhgtnu6oa";
        public Guid ProjectId { get; set; } = Guid.Parse("421987af-ee2d-4a53-99f3-b5b4e017cae9");

        public IDisposable GetConnection()
        {
            var clientCredentials = new VssBasicCredential(string.Empty, PAT);
            return new VssConnection(ServerUrl, clientCredentials);

        }
    }

}
