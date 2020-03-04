using System.Collections.Generic;
using System.IO;
using Kickstart.Pass1.KModel;

namespace Kickstart.Pass3
{
    public class CodeGenerator : ICodeGenerator
    {
        private IVisualStudioSolutionWriter _visualStudioSolutionWriter;
        public CodeGenerator(IVisualStudioSolutionWriter visualStudioSolutionWriter)
        {
            _visualStudioSolutionWriter = visualStudioSolutionWriter;
        }
        public void GenerateCode(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            {
                var parentPath = Path.Combine(outputRootPath, solution.SolutionName);
                _visualStudioSolutionWriter.Write(parentPath, solution.GeneratedSolution);

                _visualStudioSolutionWriter.Write(parentPath, solution.GeneratedTestSolution);

                if (solution is KApplicationSolution)
                {
                    var appSolution = solution as KApplicationSolution;
                    var codeWriterChild = _visualStudioSolutionWriter;// new VisualStudioSolutionWriter();

                    foreach (var childSolution in appSolution.ChildSolution)
                    {
                        var childPath = Path.Combine(parentPath, childSolution.SolutionName);
                        codeWriterChild.Write(childPath, childSolution.GeneratedSolution);

                        codeWriterChild.Write(childPath, childSolution.GeneratedTestSolution);
                    }
                }
            }
        }


    }
}
