using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2.CModel.Git
{
    public class CRepo : CPart
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public IList<string> SolutionWhiteList { get; set; } = new List<string>();

        public IList<CSolution> RepoSolution { get; set; } = new List<CSolution>();
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
