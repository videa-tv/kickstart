using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KDockerBuildScriptProject : KProject
    {
        public KDockerBuildScriptProject()
        {
            ProjectIs = CProjectIs.DockerBuildScripts;
            ProjectSuffix = "Build";
        }
    }
}