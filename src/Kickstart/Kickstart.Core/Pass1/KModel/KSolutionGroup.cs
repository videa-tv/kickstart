using System.Collections.Generic;

namespace Kickstart.Pass1.KModel
{
    public class KSolutionGroup : KPart
    {
        public string SolutionGroupName { get; set; }
        public List<KSolution> Solution { get; set; } = new List<KSolution>();
    }
}