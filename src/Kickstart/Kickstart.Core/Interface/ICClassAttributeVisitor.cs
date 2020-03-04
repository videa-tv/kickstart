using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICClassAttributeVisitor
    {
        void Visit(IVisitor visitor, CClassAttribute classAttribute);
    }
}