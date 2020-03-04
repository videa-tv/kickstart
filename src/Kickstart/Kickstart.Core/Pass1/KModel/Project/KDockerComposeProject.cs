using Kickstart.Pass2.CModel.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass1.KModel.Project
{
    public class KDockerComposeProject : KProject
    {
        public KDockerComposeProject()
        {
            ProjectIs = CProjectIs.DockerCompose;

            ProjectSuffix = "Docker";
        }
    }
}
