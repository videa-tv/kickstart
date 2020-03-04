using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICAssemblyInfoVisitor
    {
        void Visit(IVisitor visitor, CAssemblyInfo assemblyInfo);
    }
}