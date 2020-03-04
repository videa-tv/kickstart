
using System.Collections.Generic;
using System.Linq;
using NuGet;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Kickstart.Utility
{
    public partial class NugetQueryService : INugetQueryService
    {
        public NugetQueryService()
        {
        }

        public string GetLatestVersionNumber(string packageId, string minVersion)
        {
            
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();

            providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            var sourceRepository = Repository.CreateSource(providers, packageSource, FeedType.Undefined);

            
            try
            {

                var dependencyInfoResource = sourceRepository.GetResource<MetadataResource>();

                var packages = (dependencyInfoResource.GetVersions(packageId, false, false, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None)).Result;
                if (packages.Any(p => p.IsPrerelease == false))
                {
                    var latest = packages.Where(p => p.IsPrerelease == false).OrderByDescending(p => p.Version.Major)
                        .ThenByDescending(p => p.Minor).ThenByDescending(p => p.Patch).First();

                    return latest.OriginalVersion;
                }
                else
                {
                    /*
                    //look in Company nuget repo
                    packageSource = new PackageSource("http://nuget.company.int:8080/NuGet/main");
                    sourceRepository = Repository.CreateSource(providers, packageSource, FeedType.Undefined);

                    dependencyInfoResource = sourceRepository.GetResource<MetadataResource>();

                    packages = (dependencyInfoResource.GetVersions(packageId, false, false, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None)).Result;
                    if (packages.Any(p => p.IsPrerelease == false))
                    {
                        var latest = packages.Where(p => p.IsPrerelease == false).OrderByDescending(p => p.Version.Major)
                            .ThenByDescending(p => p.Minor).ThenByDescending(p => p.Patch).First();

                        return latest.OriginalVersion;
                    }*/
                }

                int x = 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return "";
        }
    }

    public interface INugetQueryService
    {
        string GetLatestVersionNumber(string NuGetName, string minVersion);

    }
}
