using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICInterfaceVisitor
    {
        ICodeWriter CodeWriter { get; }

        void Visit(IVisitor visitor, CInterface cclass);
    }
}