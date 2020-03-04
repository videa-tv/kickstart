using System.Collections.Generic;
using MediatR;
using Google.Protobuf.WellKnownTypes;
using Kickstart.Build.Services.Model;

namespace Kickstart.Build.Services.Query
{
    public class QueueBuildQuery : IRequest<Model.Build>
    {
        public BuildDefinition BuildDefinition { get; set; }


    }

}
