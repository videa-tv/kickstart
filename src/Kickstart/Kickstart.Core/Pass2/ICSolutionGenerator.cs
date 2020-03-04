using System.Collections.Generic;
using Kickstart.Pass1.KModel;

namespace Kickstart.Pass2
{
    public interface ICSolutionGenerator
    {
        bool GenerateCSolution(KSolution solution);
        void GenerateCSolutions(string outputRootPath, string connectionString, List<KSolutionGroup> solutionGroupList);
    }
}