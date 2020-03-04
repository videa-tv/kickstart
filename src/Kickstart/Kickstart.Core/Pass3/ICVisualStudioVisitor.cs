using Kickstart.Interface;
using Kickstart.Pass2.CModel;

namespace Kickstart.Pass3
{
    public interface ICVisualStudioVisitor : IVisitor
    {
        void Visit(CPart part);
    }
}