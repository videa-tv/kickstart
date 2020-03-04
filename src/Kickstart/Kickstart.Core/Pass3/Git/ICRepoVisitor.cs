using Kickstart.Interface;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.CModel.Git;

namespace Kickstart.Pass3.Git
{
    public interface ICRepoVisitor
    {
        void Visit(IVisitor visitor, CRepo metaRepo);
    }
}