using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICMethodAttributeVisitor
    {
        void Visit(IVisitor visitor, CMethodAttribute methodAttribute);
    }
}