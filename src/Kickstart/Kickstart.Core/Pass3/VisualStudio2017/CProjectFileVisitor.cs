using System.IO;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;
using Microsoft.Extensions.Logging;

namespace Kickstart.Pass3.VisualStudio2017
{
    public class CProjectFileVisitor : ICProjectFileVisitor
    {
        private readonly ICodeWriter _codeWriter;

        private readonly IFileWriter _fileWriter;

        //
        private readonly ILogger _logger;

        public CProjectFileVisitor(ILogger<CProjectFileVisitor> logger, IFileWriter fileWriter, ICodeWriter codeWriter)
        {
            _logger = logger;
            _fileWriter = fileWriter;
            _codeWriter = codeWriter;
        }


        public void Visit(IVisitor visitor, CFile file)
        {
            var saveCurrentPath = _fileWriter.CurrentPath;
            _fileWriter.CurrentPath = Path.Combine(_fileWriter.CurrentPath, file.Folder);
            //_fileWriter.FileName = file.FileName;

            _fileWriter.WriteFile(file.FileName, _codeWriter.ToString(), file.Encoding);

            file.WrittenToPath = _fileWriter.CurrentPath;

            _fileWriter.CurrentPath = saveCurrentPath;
            _logger.LogInformation($"Visited {GetType()}, File: {Path.Combine(_fileWriter.CurrentPath, file.FileName)}");
        }
    }
}