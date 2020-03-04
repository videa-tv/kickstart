using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CEnumValue :CPart
    { 
        public string EnumValueName { get; set; }
        public int EnumValue { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
