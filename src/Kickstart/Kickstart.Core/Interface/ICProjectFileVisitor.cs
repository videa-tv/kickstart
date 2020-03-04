using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICProjectFileVisitor
    {
        void Visit(IVisitor visitor, CFile file);
    }
}