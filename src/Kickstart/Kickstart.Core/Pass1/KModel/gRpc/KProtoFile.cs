using System.Collections.Generic;
using Kickstart.Pass2.CModel.Proto;

namespace Kickstart.Pass1.KModel
{
    public class KProtoFile : KPart
    {
        public string ProtoFileName { get; set; }
        public string CSharpNamespace { get; set; }
        public string ProtoFileFile { get; set; }

        public string ProtoFileText { get; set; }
        public IList<KProtoRpc> Rpc { get; set; } = new List<KProtoRpc>();
        public CProtoFile GeneratedProtoFile { get; set; }

        public KProtoFile()
        {

        }
    }
}