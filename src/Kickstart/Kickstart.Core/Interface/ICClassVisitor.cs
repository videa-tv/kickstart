using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICClassVisitor
    {
        ICodeWriter CodeWriter { get; }

        void Visit(IVisitor visitor, CClass cclass);
    }
}