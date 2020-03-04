using Kickstart.Interface;

namespace Kickstart.Pass2.CModel
{
    public class CText : CPart
    {
        public string Text { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}