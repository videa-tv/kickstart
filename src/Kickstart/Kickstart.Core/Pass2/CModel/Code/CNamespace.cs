using System;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CNamespace : CPart
    {
        public string Alias { get; set; }
        private string _namespaceName;
        public string NamespaceName
        {
            get
            {
                return _namespaceName;
            }
            set
            {
                _namespaceName = value;
            }
        }

        public override void Accept(IVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return NamespaceName;
        }
    }
}