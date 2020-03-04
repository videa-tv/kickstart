using System.Collections.Generic;
using System.Linq;
using ProtoAlias = Company.KickstartBuild.Services.Types;

namespace Kickstart.Build.Services
{
    public static class KickstartBuildServiceModelExtensions
    {
        public static ProtoAlias.Build ToProto(this Kickstart.Build.Services.Model.Build source)
        {
            return new ProtoAlias.Build
            {
                BuildIdentifier = source.BuildIdentifier

            };
        }
        public static ProtoAlias.ReleaseDefinition ToProto(this Kickstart.Build.Services.Model.ReleaseDefinition source)
        {
            var rd= new ProtoAlias.ReleaseDefinition
            {
                ReleaseDefinitionIdentifier = source.ReleaseDefinitionIdentifier,
                ReleaseDefinitionName = source.ReleaseDefinitionName,
                
            };
            if (source.BuildDefinitions != null)
            {
                rd.BuildDefinitions.AddRange(source.BuildDefinitions.ToProto());
            }
            return rd;
        }
        
        public static ProtoAlias.BuildDefinition ToProto(this Kickstart.Build.Services.Model.BuildDefinition source)
        {
            return new ProtoAlias.BuildDefinition
            {
                //<unknownProtoField> = source.BuildDefinitionId,
                BuildDefinitionIdentifier = source.BuildDefinitionIdentifier,
                BuildDefinitionName = source.BuildDefinitionName,
                RepoName = source.RepoName,
                RepoPath = source.RepoPath,
                
            };


        }

        public static IEnumerable<ProtoAlias.BuildDefinition> ToProto(this IEnumerable<Kickstart.Build.Services.Model.BuildDefinition> source)
        {
            return source.Select(s => s.ToProto()).ToList();


        }

    }

}
