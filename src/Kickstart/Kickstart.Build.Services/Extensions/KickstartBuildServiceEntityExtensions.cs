using System;
using System.Linq;
using Model = Company.KickstartBuild.Services.Types;
using EntityAlias = Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using EntityAlias2 = Microsoft.TeamFoundation.Build.WebApi;

namespace Kickstart.Build.Services
{
    public static class KickstartBuildServiceEntityExtensions
    {
        public static Model.ReleaseDefinition ToModel(this EntityAlias.ReleaseDefinition source)
        {
            return new Model.ReleaseDefinition
            {
                ReleaseDefinitionIdentifier = source.Id,
                ReleaseDefinitionName = source.Name
            };
        }
        public static Model.Build ToModel(this EntityAlias2.Build source)
        {
            return new Model.Build
            {
                BuildIdentifier = source.Id
            };
        }
        public static Model.BuildDefinition ToModel(this EntityAlias2.BuildDefinition source)
        {
            return new Model.BuildDefinition
            {
                BuildDefinitionIdentifier = source.Id,
                BuildDefinitionName = source.Name,
                ProjectName = source.Project.Name,
                RepoName = source.Repository.Name,
                //RepoPath = source.RepoPath,
                //ServiceName = source.ServiceName,
                //BuildName = source.BuildName
            };
        }
    }

}
