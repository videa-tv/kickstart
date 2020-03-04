using System.Collections.Generic;
using MediatR;
using Google.Protobuf.WellKnownTypes;
using Kickstart.Build.Services.Model;

namespace Kickstart.Build.Services.Query
{
    public class CreateBuildDefinitionQuery : IRequest<Model.BuildDefinition>
    {
        public string ServiceName { get; set; }
        public BuildDefinition BuildDefinition { get; set; }


    }

}
