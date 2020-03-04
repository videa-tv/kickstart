using System;
using System.Collections.Generic;

namespace Kickstart.Build.Services.Model
{
    public class BuildDefinition
    {
        public int BuildDefinitionIdentifier { get; set; }
        public string BuildDefinitionName { get; set; }
        public string ProjectName { get; set; }
        public string RepoName { get; set; }
        public string RepoPath { get; set; }
        public Guid ProjectId { get; internal set; }
    }

}
