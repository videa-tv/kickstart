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
    public class CDockerFileServiceVisitor : ICDockerFileServiceVisitor
    {
        private ILogger _logger;

        public ICodeWriter CodeWriter { get; }

        public CDockerFileServiceVisitor(ICodeWriter codeWriter, ILogger<CDockerFileServiceVisitor> logger)
        {
            _logger = logger;
            CodeWriter = codeWriter;
        }

        public void Visit(IVisitor visitor, CDockerFileService service)
        {
            CodeWriter.WriteLine($"b_{service.ServiceName}:");
            CodeWriter.Indent();
            CodeWriter.WriteLine($"image: b_{service.ServiceName}");
            CodeWriter.WriteLine($"build:");
            CodeWriter.Indent();
            CodeWriter.WriteLine($"context: {service.Context}");
            CodeWriter.WriteLine($"dockerfile: {service.DockerFileName}");
            CodeWriter.Unindent();
            CodeWriter.Unindent();
        }
    }
}
