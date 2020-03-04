using System;
using System.Text;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Git;
using Kickstart.Pass3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kickstart.App.Tests
{
    [TestClass]
    public class MetaRepoTest
    {
        [TestMethod]
        public void ShouldCreateSolutionWithMetaRepoProjectManualVerify()
        {
            //Arrange
            var metaRepo = new MetaRepoBuilder().BuildSampleMetaRepo();
            var project = new CProject() { ProjectName = "meta-repo", ProjectIs = CProjectIs.MetaRepo, ProjectType = CProjectType.MetaRepoProj } ;

            project.ProjectContent.Add(new CProjectContent { Content = metaRepo, File = new CFile { FileName = "metaproject.cmd", Encoding = Encoding.ASCII }, BuildAction = CBuildAction.None });
            var solution = new CSolution() { SolutionName = "MetaSolution" };
            solution.Project.Add(project);

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IVisualStudioSolutionWriter, VisualStudioSolutionWriter>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var writer = serviceProvider.GetRequiredService<IVisualStudioSolutionWriter>();


            //Act
            writer.Write(@"c:\temp\meta1\",solution);
            
            //Assert

            //todo: manual verification
        }
    }
}
