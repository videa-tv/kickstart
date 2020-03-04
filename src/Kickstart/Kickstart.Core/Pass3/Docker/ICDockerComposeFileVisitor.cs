using Kickstart.Interface;
using Kickstart.Pass2.CModel.Docker;

namespace Kickstart.Pass3.Docker
{
    public interface ICDockerComposeFileVisitor
    {
        void Visit(IVisitor visitor, CDockerComposeFile dockerCompose);
    }
}