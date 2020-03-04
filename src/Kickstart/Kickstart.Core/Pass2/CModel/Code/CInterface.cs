using System.Collections.Generic;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CInterface : CPart
    {
        public string InterfaceName { get; set; }
        public CNamespace Namespace { get; set; }
        public List<CMethod> Method { get; set; } = new List<CMethod>();
        public List<CNamespaceRef> NamespaceRef { get; set; } = new List<CNamespaceRef>();
        public CInterface InheritsFrom { get; set; }
        public bool IsGeneric { get; set; }

        public IList<CWhere> Where { get; set; } = new List<CWhere>();


        public override void Accept(IVisitor visitor)
        {
            visitor.VisitCInterface(this);
        }
    }
}