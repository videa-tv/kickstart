using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICConstructorVisitor
    {
        void Visit(IVisitor visitor, CConstructor constructor);
    }
}