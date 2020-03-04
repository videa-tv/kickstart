using Kickstart.Interface;
using Kickstart.Pass2.CModel.Docker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kickstart.Pass3.Docker
{
    public class CDockerComposeFileVisitor : ICDockerComposeFileVisitor
    {
        private ILogger _logger;

        public ICodeWriter CodeWriter { get; }

        public CDockerComposeFileVisitor(ICodeWriter codeWriter, ILogger<CDockerComposeFileVisitor> logger)
        {
            _logger = logger;
            CodeWriter = codeWriter;
        }

        public void Visit(IVisitor visitor, CDockerComposeFile dockerComposeFile)
        {
            CodeWriter.Clear();
            CodeWriter.WriteLine("version: '3'");

            if (dockerComposeFile.Service.Count > 0)
            {
                CodeWriter.WriteLine();
                CodeWriter.WriteLine("services:");
                CodeWriter.Indent();
            }

            foreach (var service in dockerComposeFile.Service)
            {
                service.Accept(visitor);
            }

            if (dockerComposeFile.Service.Count> 0)
            {
                CodeWriter.Unindent();
            }
        }
    }
}
