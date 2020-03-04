using System;
using System.Collections.Generic;

namespace Kickstart.Build.Services.Model
{
    public class Release
    {
        public int ReleaseIdentifier { get; set; }
        public Guid ProjectId { get; set; }

        public IEnumerable<Environment> Environments { get; set; } = new List<Environment>();

    }

}
