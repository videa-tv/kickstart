using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.CModel.Git;

namespace Kickstart.Pass3.VisualStudio2017
{
    public interface ICVisualStudioVisitorBase
    {
        void AddProjectToSolution(CProject project, string filePath);
        void Visit(CAssemblyInfo assemblyInfo);
        void Visit(CClassAttribute classAttribute);
        void Visit(CConstructor constructor);
        void Visit(CDockerComposeFile dockerComposeFile);
        void Visit(CDockerFileService service);
        void Visit(CField field);
        void Visit(CFile file);
        void Visit(CMetaRepo metaRepo);
        void Visit(CMethodAttribute methodAttribute);
        void Visit(CPart part);
        void Visit(CProperty property);
        void Visit(CRepo repo);
        void Visit(CSolution solution);
        void VisitCClass(CClass cclass);
        void VisitCInterface(CInterface sInterface);
        void VisitCMethod(CMethod method);
        void VisitCParameter(CParameter parameter);
        void VisitSProject(CProject project);
        void VisitSProjectContent(CProjectContent projectContent);
    }
}