using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CField : CPart
    {
        public string Comment { get; set; }
        public CAccessModifier AccessModifier { get; set; } = CAccessModifier.Private;
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string ReferencesType { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsConst { get; set; }
        public bool IsStatic { get; set; }
        public string DefaultValue { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}