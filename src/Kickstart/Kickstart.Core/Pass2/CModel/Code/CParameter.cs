using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CParameter : CPart
    {
        public string ParameterName { get; set; }
        public string Type { get; set; }
        public bool PassToBaseClass { get; set; }

        public string DefaultValue { get; set; }

        public bool IsScalar
        {
            get
            {
                if (Type == "int")
                    return true;
                if (Type == "string")
                    return true;
                //todo: complete this
                return false;
            }
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitCParameter(this);
        }

        public override string ToString()
        {
            return $"{Type} {ParameterName}";
        }
    }
}