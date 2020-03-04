using System.Collections.Generic;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CSolution : CPart
    {
        public string SolutionName { get; set; }
        public string SolutionPath { get; set; }
        public List<CProject> Project { get; set; } = new List<CProject>();

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}