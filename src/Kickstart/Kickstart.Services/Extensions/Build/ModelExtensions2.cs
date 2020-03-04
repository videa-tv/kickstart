using EntityAlias = Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using EntityAlias2 = Microsoft.TeamFoundation.Build.WebApi;

using System.Collections.Generic;
using System.Linq;

namespace Kickstart.Build.Services
{
    public static class ModelExtensions2
    {

        public static EntityAlias.ReleaseDefinition ToEntity(this Kickstart.Build.Services.Model.ReleaseDefinition source)
        {
            return new EntityAlias.ReleaseDefinition
            {
                Name = source.ReleaseDefinitionName,
                Id = source.ReleaseDefinitionIdentifier
            };


        }

        public static IEnumerable<EntityAlias.ReleaseDefinition> ToEntity(this IEnumerable<Kickstart.Build.Services.Model.ReleaseDefinition> source)
        {
            return source.Select(s => s.ToEntity()).ToList();


        }

        public static EntityAlias2.BuildDefinition ToEntity(this Kickstart.Build.Services.Model.BuildDefinition source)
        {
            return new EntityAlias2.BuildDefinition
            {
                Id = source.BuildDefinitionIdentifier,
                Name = source.BuildDefinitionName,
                Project = new Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference
                {
                    Id = source.ProjectId,
                    Name = source.ProjectName
                },
                Repository = new EntityAlias2.BuildRepository
                {
                    Name = source.RepoName
                },
                //RepoPath = source.RepoPath,
                //ServiceName = source.ServiceName,
                //BuildName = source.BuildName

            };


        }

        public static IEnumerable<EntityAlias2.BuildDefinition> ToEntity(this IEnumerable<Kickstart.Build.Services.Model.BuildDefinition> source)
        {
            return source.Select(s => s.ToEntity()).ToList();


        }

        public static EntityAlias.Release ToEntity(this Kickstart.Build.Services.Model.Release source)
        {
            return new EntityAlias.Release
            {
                Id = source.ReleaseIdentifier

            };


        }

        public static IEnumerable<EntityAlias.Release> ToEntity(this IEnumerable<Kickstart.Build.Services.Model.Release> source)
        {
            return source.Select(s => s.ToEntity()).ToList();


        }

    }

}
