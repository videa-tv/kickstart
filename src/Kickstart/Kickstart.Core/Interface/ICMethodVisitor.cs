using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICMethodVisitor
    {
        void Visit(IVisitor visitor, CMethod method);
    }
}