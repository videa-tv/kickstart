using Kickstart.Pass1.KModel;

namespace Kickstart.Pass1.Service
{
    public interface IProtoToKProtoConverter
    {
        KProtoFile Convert(string protoFileName, string protoFileContent);
    }
}