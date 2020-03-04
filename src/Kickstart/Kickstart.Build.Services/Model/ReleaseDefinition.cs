using System;
using System.Collections.Generic;

namespace Kickstart.Build.Services.Model
{
    public class ReleaseDefinition
    {
        public string ReleaseDefinitionName { get; set; }
        public int ReleaseDefinitionIdentifier { get; set; }

        public Guid ProjectId { get; internal set; }

        public IEnumerable<BuildDefinition> BuildDefinitions { get; set; }

        public IEnumerable<Environment> Environments { get; set; }

    }

}
