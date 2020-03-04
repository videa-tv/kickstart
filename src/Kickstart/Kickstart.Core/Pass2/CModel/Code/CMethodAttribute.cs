using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CMethodAttribute : CPart
    {
        public string AttributeName { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}