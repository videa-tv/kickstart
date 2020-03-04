using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICProjectContentVisitor
    {
        void Visit(IVisitor visitor, CProjectContent projectContent);
    }
}