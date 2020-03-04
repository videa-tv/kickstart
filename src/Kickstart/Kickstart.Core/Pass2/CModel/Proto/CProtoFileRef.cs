using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Proto
{
    public class CProtoFileRef : CPart
    {
        public CProtoFile ProtoFile { get; set; }


        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}