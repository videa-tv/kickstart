using Kickstart.Pass2.CModel.Proto;

namespace Kickstart.Pass1.KModel
{
    public class KProtoRef : KPart
    {
        public string ServiceName { get; set; }
        public CProtoRpcRefDataDirection Direction { get; set; }
        public string RefServiceName { get; set; }
        public string RefRpcName { get; set; }
        public KSolution RefSolution { get; set; }
    }
}