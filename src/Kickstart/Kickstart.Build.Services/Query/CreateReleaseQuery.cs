using System.Collections.Generic;
using MediatR;
using Google.Protobuf.WellKnownTypes;
using Kickstart.Build.Services.Model;

namespace Kickstart.Build.Services.Query
{
    public class CreateReleaseQuery : IRequest<bool>
    {
        public ReleaseDefinition ReleaseDefinition { get; set; }

    }

}
