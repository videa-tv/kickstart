using System;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CNamespaceRef : CPart
    {
        private CNamespace _referenceTo;
        
        public CNamespaceRef(CNamespace @namespace)
        {
            ReferenceTo = @namespace;
        }

        public CNamespaceRef(string nameSpaceName)
        {
            ReferenceTo = new CNamespace() {NamespaceName = nameSpaceName};
        }

        public CNamespaceRef()
        {
        }


        public CNamespace ReferenceTo
        {
            get
            {
                return _referenceTo;
            }
            set
            {
                if (_referenceTo !=null)
                {
                    int x = 1;
                }
                _referenceTo = value;
            }
        }

        public override void Accept(IVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"Ref-->{ReferenceTo.NamespaceName}";
        }
    }
}