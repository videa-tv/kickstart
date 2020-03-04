using System.Collections.Generic;

namespace Kickstart.Pass1.KModel
{
    /// <summary>
    ///     composite multiple solutions togther into an application
    /// </summary>
    public class KApplicationSolution : KSolution
    {
        public IList<KSolution> ChildSolution { get; set; } = new List<KSolution>();
        public bool NestChildSolutionFolders { get; set; } = true;
    }
}