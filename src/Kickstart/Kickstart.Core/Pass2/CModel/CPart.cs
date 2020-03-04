using Kickstart.Interface;
using Newtonsoft.Json;

namespace Kickstart.Pass2.CModel
{
    public abstract class CPart
    {
        [JsonIgnore]
        public bool Kickstart { get; set; } = true;
        [JsonIgnore]
        public  CPart DerivedFrom { get; set; }
        public abstract void Accept(IVisitor visitor);
    }
}