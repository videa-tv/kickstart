using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.Docker
{
    public interface IBuildScriptService
    {
        CProject Execute(string solutionName, KDockerBuildScriptProject mDockerBuildScriptProject);
    }
}