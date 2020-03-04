using System.Diagnostics;
using Kickstart.Interface;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass3.CSharp;
using Kickstart.Pass3.VisualStudio2017;
using Kickstart.Utility;
using Microsoft.Extensions.Logging;

namespace Kickstart.Pass3
{
    public class VisualStudioSolutionWriter : IVisualStudioSolutionWriter
    {
        private readonly ILogger _logger;
        private readonly ICodeWriter _codeWriter;
        private readonly ICVisualStudioVisitor _visualStudioVisitor;
        private readonly IFileWriter _fileWriter;

        public VisualStudioSolutionWriter(ILogger<VisualStudioSolutionWriter> logger, IFileWriter fileWriter, ICodeWriter codeWriter,  ICVisualStudioVisitor visualStudioVisitor)
        {
            _logger = logger;
            _codeWriter = codeWriter;
            _visualStudioVisitor = visualStudioVisitor;
            _fileWriter = fileWriter;
        }
        
        public void Write(string outputRootPath, CSolution solution)
        {
            _fileWriter.RootPath = outputRootPath;
            /*
            var visitor = new CVisualStudioVisitor(_logger, codeWriter,
                new FastSolutionVisitor(new CodeWriter(), fileWriter),
                new CProjectVisitor(_logger, new CodeWriter(), fileWriter),
                new CProjectFileVisitor(_logger, fileWriter, codeWriter), new CSharpCInterfaceVisitor(codeWriter),
                new CSharpCClassVisitor(codeWriter), new CSharpCMethodVisitor(codeWriter),
                new CSharpCPropertyVisitor(codeWriter),
                new CSharpCParameterVisitor(codeWriter),
                new CSharpCFieldVisitor(codeWriter),
                new CSharpCConstructorVisitor(codeWriter),
                null,
                new CSharpCClassAttributeVisitor(codeWriter),
                new CSharpCMethodAttributeVisitor(codeWriter));
                */
            solution.Accept(_visualStudioVisitor);

            //now that the .proto files and .cmd have been written to disk
            //execute the .cmd to generate the .cs files

            foreach (var project in solution.Project)
            foreach (var pc in project.ProjectContent)
                if (pc.Content is CBatchFile)
                {
                        var batchFile = pc.Content as CBatchFile;
                        if (!batchFile.ExecutePostKickstart)
                            continue;
                        //todo: this probably won't run in Docker / Linux
                    var processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = pc.File.FileName;
                    processStartInfo.WorkingDirectory = pc.File.WrittenToPath;
                    processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processStartInfo.UseShellExecute = true;
                    Process.Start(processStartInfo);

                    //todo: verify success
                }
        }
    }
}