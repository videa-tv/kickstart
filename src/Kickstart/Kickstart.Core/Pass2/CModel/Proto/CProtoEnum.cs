using System.Collections.Generic;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Proto
{
    public class CProtoEnum : CPart
    {
        public string EnumName { get; set; }

        public List<CProtoEnumValue> EnumValue { get; set; } = new List<CProtoEnumValue>();

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}