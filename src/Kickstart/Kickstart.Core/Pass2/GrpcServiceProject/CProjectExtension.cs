using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public static class CProjectExtension
    {
        /*
        public static List<SProtoRpcRef> GetProtoRpcRefs(this SProject project)
        {
            return project.ProjectContent.Where(pc => pc.Content is SProtoRpcRef).Select(pc => pc.Content as SProtoRpcRef).ToList();

        }
        */
        /*
        public static List<SProtoFileRef> GetProtoFileRefs(this SProject project)
        {
            return project.ProjectContent.Where(pc => pc.Content is SProtoFileRef).Select(pc => pc.Content as SProtoFileRef).ToList();

        }
        */
        public static List<CProtoFile> GetProtoFiles(this CProject project)
        {
            return project.ProjectContent.Where(pc => pc.Content is CProtoFile).Select(pc => pc.Content as CProtoFile)
                .ToList();
        }

        public static CProtoFile GetProtoFile(this CProject project, string serviceName)
        {
            foreach (var pc in project.ProjectContent)
                if (pc.Content is CProtoFile)
                {
                    var protoFile = pc.Content as CProtoFile;

                    if (protoFile.ProtoService.Exists(s => s.ServiceName == serviceName))
                        return protoFile;
                }

            return null;
        }
    }
}