using Kickstart.Interface;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.CModel.Code;
using System.Collections.Generic;

namespace Kickstart.Pass1.KModel
{
    public class KProject : KPart
    {

        public IList<KProject> DependsOnProject { get; set; } = new List<KProject>();
        public bool Kickstart { get; set; } = true;
        public string CompanyName { get; set; }
        public string NamespaceSuffix { get; set; } = ""; //"NS"
        public string ProjectName { get; set; }
        public string ProjectNameAsClassNameFriendly { get { return ProjectName.Replace(".", ""); } }

        public string ProjectShortName => $"{ProjectName}.{ProjectSuffix}";

        public string ProjectSuffix { get; set; }
        public string ProjectSuffixSuffix { get; set; }
        public string ProjectFullName
        {
            get
            {
                if (!string.IsNullOrEmpty(ProjectSuffixSuffix))
                {
                    return $"{CompanyName}.{ProjectName}.{ProjectSuffix}.{ProjectSuffixSuffix}";
                }
                return $"{CompanyName}.{ProjectName}.{ProjectSuffix}";

            }
        }

        public string ProjectFolderBase { get; set; } = string.Empty;

        public string ProjectFolder
        {
            get
            {
                var baseFolder = string.Empty;
                if (!string.IsNullOrEmpty(ProjectFolderBase))
                    baseFolder = $"{ProjectFolderBase}\\";
                //todo: don't hard code this, even though its reading the flags
                if (ProjectIs.HasFlag(CProjectIs.SolutionFiles))
                    return baseFolder; // string.Empty;
                if (ProjectIs.HasFlag(CProjectIs.DockerBuildScripts))
                    return $@"{baseFolder}build\{CompanyName}.{ProjectName}.{ProjectSuffix}";
                if (ProjectIs.HasFlag(CProjectIs.Test))
                    return $@"{baseFolder}test\{CompanyName}.{ProjectName}.{ProjectSuffix}";
                return $@"{baseFolder}src\{CompanyName}.{ProjectName}.{ProjectSuffix}";
            }
        }

        public CProjectIs ProjectIs { get; set; }
        public DotNetType DotNetType { get; set; } = DotNetType.Core;

        public IMetadataConfigService MetadataConfigService { get; set; }
        public ISeedDataService SeedDataService { get; set; }
        
        public virtual void ConfigureMetaData()
        {
        }


        public virtual void ConfigureMetaData2()
        {
            //some stuff doesn't created till after ConfigureMetaData()
        }
        public CProject GeneratedProject { get; set; }
    }
}