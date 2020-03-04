using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICFieldVisitor
    {
        void Visit(CField field);
    }
}