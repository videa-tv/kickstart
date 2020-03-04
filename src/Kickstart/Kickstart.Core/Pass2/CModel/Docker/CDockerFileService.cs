using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Docker
{
    public class CDockerFileService : CPart
    {
        public string ServiceName { get; internal set; }
        public string DockerFileName { get; internal set; } = "Dockerfile";
        public string Context { get; internal set; } = ".";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
