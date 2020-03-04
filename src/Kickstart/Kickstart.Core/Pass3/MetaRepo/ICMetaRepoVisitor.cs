using Kickstart.Interface;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.CModel.Git;

namespace Kickstart.Pass3.Docker
{
    public interface ICMetaRepoVisitor
    {
        void Visit(IVisitor visitor, CMetaRepo metaRepo);
    }
}