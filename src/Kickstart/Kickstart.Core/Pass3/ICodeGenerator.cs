using System.Collections.Generic;
using Kickstart.Pass1.KModel;

namespace Kickstart.Pass3
{
    public interface ICodeGenerator
    {
        void GenerateCode(List<KSolutionGroup> solutionGroupList, string outputRootPath);
    }
}