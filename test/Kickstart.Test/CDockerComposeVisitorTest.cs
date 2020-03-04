using System;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass3.Docker;
using Kickstart.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Kickstart.Test
{
    [TestClass]
    public class CDockerComposeVisitorTest
    {
        [TestMethod]
        public void FileShouldHave1Line()
        {
            //Arrange
            var dockerCompose = new CDockerComposeFile();
            var codeWriter = new CodeWriter();
            var logger = new Mock<ILogger<CDockerComposeFileVisitor>>();
            var vi = new Mock<IVisitor>();
            var visitor = new CDockerComposeFileVisitor(codeWriter, logger.Object);

            //Act
            visitor.Visit(vi.Object, dockerCompose);

            //Assert

            Assert.AreEqual(1, codeWriter.LineCount);
        }
    }
}
