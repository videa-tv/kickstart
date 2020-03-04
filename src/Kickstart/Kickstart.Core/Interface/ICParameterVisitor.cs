using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICParameterVisitor
    {
        void Visit(CParameter parameter);
    }
}