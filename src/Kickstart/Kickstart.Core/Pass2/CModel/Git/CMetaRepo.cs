using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Git
{
    public class CMetaRepo : CPart
    {
        public string MetaRepoName { get; set; }
        public CRepo CompositeRepo { get; set; }
        public List<CRepo> Repos { get;  } = new List<CRepo>();
        public string CheckoutFromBranch { get; internal set; } = "dev";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void AddRepos(List<CRepo> repos)
        {
            foreach (var repo in repos)
            {
                if (this.Repos.Exists(r => r.Name == repo.Name))
                {
                    continue;
                }
                Repos.Add(repo);
            }
        }
    }
}
