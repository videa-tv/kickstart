using System;

namespace Kickstart.Build2
{
    public interface ITfsConnectInfo
    {
        string PAT { get; set; }
        Guid ProjectId { get; set; }
        Uri ServerUrl { get; set; }
    }
}