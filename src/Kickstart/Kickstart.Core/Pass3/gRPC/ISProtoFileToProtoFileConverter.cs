using Kickstart.Pass2.CModel.Proto;

namespace Kickstart.Pass3.gRPC
{
    public interface ISProtoFileToProtoFileConverter
    {
        string Convert(CProtoFile protoFile);
    }
}