using Kickstart.Interface;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.CModel.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
namespace Kickstart.Pass3.Git
{
    public class CRepoVisitor : ICRepoVisitor
    {
        private ILogger _logger;

        public ICodeWriter CodeWriter { get; }

        public CRepoVisitor(ICodeWriter codeWriter, ILogger<CRepoVisitor> logger)
        {
            _logger = logger;
            CodeWriter = codeWriter;
        }

        public void Visit(IVisitor visitor, CRepo repo)
        {
            CodeWriter.WriteLine($@"CMD /c "" meta project add {repo.Name} {repo.Url}{repo.Name}");
        }
    }
}
