using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CAssemblyInfo : CPart
    {
        public string AssemblyCompany { get; set; } = "CompanyName";
        public string AssemblyVersion { get; set; } = "1.0.191";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}