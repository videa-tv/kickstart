using Kickstart.Interface;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.CModel.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Kickstart.Pass3.Docker
{
    public class CMetaRepoVisitor : ICMetaRepoVisitor
    {
        private ILogger _logger;

        public ICodeWriter CodeWriter { get; }

        public CMetaRepoVisitor(ICodeWriter codeWriter, ILogger<CMetaRepoVisitor> logger)
        {
            _logger = logger;
            CodeWriter = codeWriter;
        }

        public void Visit(IVisitor visitor, CMetaRepo metaRepo)
        {
            CodeWriter.Clear();
            CodeWriter.WriteLine("call npm install -g meta");
            CodeWriter.WriteLine();

            CodeWriter.WriteLine("call meta init");
            CodeWriter.WriteLine();

            //CodeWriter.WriteLine("setlocal enableDelayedExpansion");
            foreach (var repo in metaRepo.Repos)
            {
                repo.Accept(visitor);
            }
            //CodeWriter.WriteLine("endlocal");
            CodeWriter.WriteLine();
            CodeWriter.WriteLine($"CMD /c meta git fetch --all");
            CodeWriter.WriteLine($"CMD /c meta git checkout --track remotes/origin/{metaRepo.CheckoutFromBranch}");

            CodeWriter.WriteLine($"CMD /c meta git pull");
            CodeWriter.WriteLine($"CMD /c meta git checkout Poc_XXXXXX");

        }
    }
}
