using Kickstart.Interface;
using Kickstart.Pass2.CModel.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Kickstart.Pass3.MetaRepo
{
    public class CompositeSolutionVisitor 
    {
        private ILogger _logger;

        public ICodeWriter CodeWriter { get; }

        public CompositeSolutionVisitor(ICodeWriter codeWriter, ILogger<CompositeSolutionVisitor> logger)
        {
            _logger = logger;
            CodeWriter = codeWriter;
        }

        public void Visit(IVisitor visitor, CMetaRepo metaRepo)
        {
            foreach (var repo in metaRepo.Repos)
            {
                foreach (var solution in repo.RepoSolution)
                {
                    foreach (var project in solution.Project)
                    {
                        CodeWriter.WriteLine($@"dotnet sln Company.All.Sln add ""{project.Path}""");
                    }
                }
            }

        }
    }
}
