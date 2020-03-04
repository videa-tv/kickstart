using System;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Utility;

namespace Kickstart.Pass2.UnitTestProject
{
    public interface IGrpcClientProjectService
    {
        CProject BuildProject(KSolution kSolution, KGrpcClientProject grpcClientKProject);
    }

    public class GrpcClientProjectService : IGrpcClientProjectService
    {   
        private KGrpcClientProject _grpcClientKProject;

        public CProject BuildProject(KSolution kSolution, KGrpcClientProject grpcClientKProject)
        {
            _grpcClientKProject = grpcClientKProject;
            var project = new CProject
            {
                ProjectName = grpcClientKProject.ProjectFullName,
                ProjectShortName = grpcClientKProject.ProjectShortName,
                ProjectFolder = grpcClientKProject.ProjectFolder,
                ProjectType = CProjectType.CsProj,
                ProjectIs = grpcClientKProject.ProjectIs
            };

            project.TemplateProjectPath = @"templates\NetStandard20ClassLibrary.csproj";
            
            return project;
        }
        
    }
}