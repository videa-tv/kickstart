using System;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.DockerComposeProject;
using Kickstart.Utility;

namespace Kickstart.Pass2.UnitTestProject
{
    public interface IDockerComposeProjectService
    {
        CProject BuildProject(KSolution kSolution, KDockerComposeProject dockerComposeKProject);
    }

    public class DockerComposeProjectService : IDockerComposeProjectService
    {   
        private KDockerComposeProject _dockerComposeKProject;

        public CProject BuildProject(KSolution kSolution, KDockerComposeProject dockerComposeKProject)
        {
            _dockerComposeKProject = dockerComposeKProject;
            var project = new CProject
            {
                ProjectName = dockerComposeKProject.ProjectFullName,
                ProjectShortName = dockerComposeKProject.ProjectShortName,
                ProjectFolder = dockerComposeKProject.ProjectFolder,
                ProjectType = CProjectType.DockerProj,
                ProjectIs = dockerComposeKProject.ProjectIs
            };

            project.TemplateProjectPath = @"templates\Docker\docker-compose.dcproj";

            var dockerComposeFile = new DockerComposeFileService().Build();

            project.ProjectContent.Add(new CProjectContent { Content = dockerComposeFile, File = new CFile { FileName = "docker-compose.yml" }, BuildAction = CBuildAction.None });

            return project;
        }
        
    }
}