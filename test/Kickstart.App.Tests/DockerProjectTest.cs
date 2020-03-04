using System;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.DockerComposeProject;
using Kickstart.Pass3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kickstart.App.Tests
{
    [TestClass]
    public class DockerProjectTest
    {
        [TestMethod]
        public void ShouldCreateSolutionWithDockerComposeProjectManualVerify()
        {
            //Arrange
            
         
            var metaRepo = new MetaRepoBuilder().BuildSampleMetaRepo();
            var dockerComposeFile = new DockerComposeFileService().Build(metaRepo);

            var project = new CProject() { ProjectName = "docker-compose", ProjectIs = CProjectIs.DockerCompose, ProjectType = CProjectType.DockerProj, TemplateProjectPath = @"templates\Docker\docker-compose.dcproj" } ;

            project.ProjectContent.Add(new CProjectContent { Content = dockerComposeFile, File = new CFile { FileName = "docker-compose.yml" }, BuildAction = CBuildAction.None });
            var solution = new CSolution() { SolutionName = "MetaSolution" };
            solution.Project.Add(project);

           
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IVisualStudioSolutionWriter, VisualStudioSolutionWriter>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var writer = serviceProvider.GetRequiredService<IVisualStudioSolutionWriter>();

            //Act
            writer.Write(@"c:\temp\",solution);
            
            //Assert
                
            //todo: manual verification
        }
    }
}
