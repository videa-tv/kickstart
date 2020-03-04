using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Docker
{
    public class CDockerComposeFile : CPart
    {
        public IList<CDockerFileService> Service { get; set; } = new List<CDockerFileService>();
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
