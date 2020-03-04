using System;
using System.IO;
using Kickstart.Interface;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.DockerComposeProject;
using Kickstart.Pass3;
using Kickstart.Pass3.MetaRepo;
using Kickstart.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
namespace Kickstart.App.Tests
{
    [TestClass]
    public class AddProjectToSlnBatchScriptTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Arrange
            var metaRepo = new MetaRepoBuilder().BuildSampleMetaRepo();

            //var batchFile = new CBatchFile() { BatchFileContent = "" };
            //var content = new CProjectContent { Content = batchFile, File = new CFile { FileName = "AddProjectsToCompositeSln" }, BuildAction = CBuildAction.None };

            var codeWriter = new CodeWriter();
            var logger = new Mock<ILogger<CompositeSolutionVisitor>>();
            var visitor = new CompositeSolutionVisitor(codeWriter, logger.Object);

            var iv = new Mock<IVisitor>();
            //Act
            visitor.Visit(iv.Object, metaRepo);

            //Assert
            File.WriteAllText(@"c:\temp\AddProjectsToCompositeSln.cmd", codeWriter.ToString());
            //todo: manual verification
        }
    }
}
