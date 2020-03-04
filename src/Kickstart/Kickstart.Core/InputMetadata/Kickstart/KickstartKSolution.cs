using System.Diagnostics.CodeAnalysis;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;

namespace Kickstart.InputMetadata.Agency
{
    [ExcludeFromCodeCoverage]
    public class KickstartKSolution : KSolution
    {
        public KickstartKSolution()
        {
            
            ProtoFolder = "Kickstart";
            ProtoFileName = "Kickstart.proto";
            //var kProtoFile = new ProtoToKProtoConverter(null).BuildProtoFileFromKSolution(this);
        }
        
    }
}