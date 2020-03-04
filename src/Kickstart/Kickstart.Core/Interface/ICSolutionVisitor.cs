using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICSolutionVisitor
    {
        void Visit(IVisitor visitor, CSolution project);
        void AddProjectToSolution(CProject project, string filePath);
    }
}