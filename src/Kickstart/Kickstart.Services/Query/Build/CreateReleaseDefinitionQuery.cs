using System.Collections.Generic;
using MediatR;
using Google.Protobuf.WellKnownTypes;
using Kickstart.Build.Services.Model;
using System.Linq;

namespace Kickstart.Build.Services.Query
{
    public class CreateReleaseDefinitionQuery : IRequest<Model.ReleaseDefinition>
    {
        public string ServiceName { get; set; }
        public string ReuseAwsStackFromServiceName { get; set; }

        public string ConfigurationFolder { get; set; }
        public string ServiceFolder { get; set; }

        public bool IsProdPath { get; set; }
        public ReleaseDefinition ReleaseDefinition { get; set; }

        public IEnumerable<DatabaseServer> DatabaseServers { get; set; }
        public bool NeedsAWS { get {
                return DatabaseServers.Any(s => s.Location == Model.ServerLocation.AWS);
            } }

        public bool NeedsAWSAurora { get {
                return DatabaseServers.Any(s => s.Location == Model.ServerLocation.AWS &&
                    (s.DbmsType == Model.DbmsType.Postgresql || s.DbmsType == Model.DbmsType.MySql));
            } }

        public bool NeedsDCOS { get; internal set; } = true;
        public bool NeedsAwsUserAccountCreated { get; internal set; } = false;
    }

}
