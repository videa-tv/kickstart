using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface IVisitor
    {
        void Visit(CPart part);
        void VisitSProject(CProject project);

        void Visit(CSolution solution);
        void AddProjectToSolution(CProject project, string filePath);
        void VisitCInterface(CInterface sInterface);
        void Visit(CFile file);

        void VisitSProjectContent(CProjectContent projectContent);

        void VisitCClass(CClass cclass);

        void VisitCMethod(CMethod method);

        void Visit(CProperty property);

        void Visit(CField field);
        void VisitCParameter(CParameter parameter);
        void Visit(CConstructor constructor);
        void Visit(CAssemblyInfo assemblyInfo);
    }
}