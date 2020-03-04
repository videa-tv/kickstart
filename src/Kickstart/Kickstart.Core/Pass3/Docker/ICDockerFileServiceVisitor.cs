using Kickstart.Interface;
using Kickstart.Pass2.CModel.Docker;

namespace Kickstart.Pass3.Docker
{
    public interface ICDockerFileServiceVisitor
    {
        ICodeWriter CodeWriter { get; }

        void Visit(IVisitor visitor, CDockerFileService service);
    }
}