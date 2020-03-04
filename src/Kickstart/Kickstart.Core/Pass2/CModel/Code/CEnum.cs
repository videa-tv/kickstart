using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CEnum :CPart
    {
        public CNamespace Namespace { get; set; }
        public string EnumName { get; set; }
        public IList<CEnumValue> EnumValues { get; set; } = new List<CEnumValue>();
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
