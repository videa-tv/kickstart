namespace Kickstart.Pass2.GrpcServiceProject
{
    public interface IGrpcPortService
    {
        int GeneratePortNumber(string serviceName);
    }
}