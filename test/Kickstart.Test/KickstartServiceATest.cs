using System;
using Kickstart.Interface;
using Kickstart.Pass2;
using Kickstart.Pass3;
using Kickstart.Pass3.VisualStudio2017;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;

namespace Kickstart.Test
{
    [TestClass]
    public class KickstartServiceATest
    {
        [TestMethod]
        public void TestMethod1()
        {
            /*
            ILogger<VisualStudioSolutionWriter> logger = null;
            ICodeWriter codeWriter = null;
            IFileWriter fileWriter = null;
            var target = new KickstartService(new CSolutionGenerator(new KSolutionToCSolutionConverter(logger)),
                new CodeGenerator(new VisualStudioSolutionWriter(logger, codeWriter, new CVisualStudioVisitor(logger, codeWriter))), 
                new FastSolutionVisitor(codeWriter, fileWriter, new VisualStudioSolutionWriter(logger, codeWriter, new CVisualStudioVisitor(logger, codeWriter) )));
            // target.Execute();
            */
        }
    }
}
