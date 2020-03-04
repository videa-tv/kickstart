using System.Collections.Generic;
using Kickstart.Interface;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.VisualStudio2017
{
    public interface IFastSolutionVisitor
    {
        void AddAllProjectsToMasterSln(CSolution solution, string outputRootPath, string solutionName);
        void AddAllProjectsToMasterSln(KSolution solution, string outputRootPath, string solutionName);
        void AddAllProjectsToMasterSln(KSolutionGroup solutionGroup, string outputRootPath, string solutionName);
        void AddAllProjectsToMasterSln(List<KSolutionGroup> solutionGroupList, string outputRootPath, string solutionName);
        void AddProjectsToApplication(List<KSolutionGroup> solutionGroupList, string outputRootPath);
        void AddProjectToSolution(CProject project, string filePath);
        void Visit(IVisitor visitor, CSolution solution);
    }
}