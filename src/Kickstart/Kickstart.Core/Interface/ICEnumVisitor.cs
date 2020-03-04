using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICEnumVisitor
    {
        ICodeWriter CodeWriter { get; }

        void Visit(IVisitor visitor, CEnum cenum);
    }
}