using System.Collections.Generic;
using MediatR;
using Google.Protobuf.WellKnownTypes;
using Kickstart.Build.Services.Model;
using System;

namespace Kickstart.Build.Services.Query
{
    public class DeployReleaseQuery : IRequest<bool>
    {
        public int ReleaseIdentifier { get; set; }
        public int EnvironmentIdentifier { get; set; }
        public Guid ProjectId { get; set; }

    }

}
