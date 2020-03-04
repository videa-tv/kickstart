using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CProperty : CPart
    {
        public CAccessModifier AccessModifier { get; set; } = CAccessModifier.Public;
        public bool IsStatic { get; set; }
        public string PropertyName {
            get;
            set;
        }
        public string Type { get; set; }

        public string ReferencesType { get; set; }
        public string DefaultValue { get; set; }
        public int MaxLength { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{Type} {PropertyName}";
        }
    }
}