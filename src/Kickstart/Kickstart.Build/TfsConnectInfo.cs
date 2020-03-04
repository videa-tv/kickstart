using System;
using System.Collections.Generic;
using System.Text;

namespace Kickstart.Build2
{

    public class TfsConnectInfo : ITfsConnectInfo
    {
        public Uri ServerUrl { get; set; } = new Uri("https://tfs.company.com/tfs/Company/");
        public string PAT { get; set; } = "todo";
        public Guid ProjectId { get; set; } = Guid.Parse("todo");

    }
}
