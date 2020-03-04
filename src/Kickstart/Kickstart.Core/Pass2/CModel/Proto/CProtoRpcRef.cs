using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Proto
{
    public class CProtoRpcRef : CPart
    {
        public CProtoRpc ProtoRpc { get; set; }
        public CProtoRpcRefDataDirection Direction { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}