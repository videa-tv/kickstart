using System.Collections.Generic;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;

namespace Kickstart.Pass1.KModel
{
    public class KGrpcProject : KProject
    {
        //public string BulkStoreRpcName { get; set; }

        public KGrpcProject()
        {
            ProjectIs = CProjectIs.Grpc;
            ProjectSuffix = "Services";
        }

        public IList<KProtoFile> ProtoFile { get; set; } = new List<KProtoFile>();
        public bool KickstartCRUD { get; set; } = false;
        public bool KickstartBulkStore { get; set; } = true;

        // public SProtoFile ProtoFile { get; set; }

        public override void ConfigureMetaData()
        {
            /*
            if (ProtoFile.Count ==0)
            {
                ProtoFile.Add(new MProtoFile());
                return;
            }
            */
            foreach (var mProtoFile in ProtoFile)
            {
                if (mProtoFile.GeneratedProtoFile != null)
                    continue;
                mProtoFile.GeneratedProtoFile = CProtoFile.FromJson(mProtoFile.ProtoFileText);
            }
        }
    }
}