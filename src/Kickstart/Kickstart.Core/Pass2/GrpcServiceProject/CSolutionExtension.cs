using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public static class CSolutionExtension
    {
        /*
        public static SProtoRpc GetProtoRpc(this CSolution solution, string serviceName, string rpcName)
        {
            foreach (var project in solution.Project)
            {
                foreach (var pc in project.ProjectContent)
                {
                    if (pc.Content is SProtoFile)
                    {
                        var protoFile = pc.Content as SProtoFile;

                        var protoService = protoFile.ProtoService.FirstOrDefault(s => s.ServiceName == serviceName);
                        if (protoService == null)
                        {
                            continue;
                        }

                        var protoRpc = protoService.Rpc.FirstOrDefault(r => r.RpcName == rpcName);

                        if (protoRpc != null)
                        {

                            return protoRpc;
                        }
                    }
                }
            }
            return null;
        }*/

        public static CProtoFile GetProtoFile(this CSolution solution, string serviceName)
        {
            foreach (var project in solution.Project)
            foreach (var pc in project.ProjectContent)
                if (pc.Content is CProtoFile)
                {
                    var protoFile = pc.Content as CProtoFile;

                    if (protoFile.ProtoService.Exists(s => s.ServiceName == serviceName))
                        return protoFile;
                }
            return null;
        }

        public static CProtoFileRef GetProtoFileRef(this CSolution solution, string serviceName)
        {
            foreach (var project in solution.Project)
            foreach (var pc in project.ProjectContent)
                if (pc.Content is CProtoFileRef)
                {
                    var protoFileRef = pc.Content as CProtoFileRef;

                    if (protoFileRef.ProtoFile.ProtoService.Exists(s => s.ServiceName == serviceName))
                        return protoFileRef;
                }
            return null;
        }
    }
}