using System.Collections.Generic;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcIntegrationProject : KGrpcProject
    {
        public KGrpcIntegrationProject()
        {
            ProjectIs = CProjectIs.Integration;
        }

        public IList<KProtoRef> ProtoRef { get; set; } = new List<KProtoRef>();

        public void AddProtoRef(string serviceName, CProtoRpcRefDataDirection direction, KSolution refSolution,
            string refServiceName, string refRpcName)
        {
            var protoRef = new KProtoRef
            {
                ServiceName = serviceName,
                Direction = direction,
                RefSolution = refSolution,
                RefServiceName = refServiceName,
                RefRpcName = refRpcName
            };
            ProtoRef.Add(protoRef);
        }
    }
}