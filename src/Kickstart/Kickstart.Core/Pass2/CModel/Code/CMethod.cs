using System.Collections.Generic;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CMethod : CPart
    {
        public List<CMethodAttribute> Attribute { get; set; } = new List<CMethodAttribute>();
        public CAccessModifier AccessModifier { get; set; } = CAccessModifier.Public;

        public bool UseExpressionDefinition { get; set; }
        public bool SignatureOnly { get; set; }
        public bool IsOverride { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAsync { get; set; }
        public bool IsExtensionMethod { get; set; }
        public string ReturnType { get; set; }
        public string ReferencesType { get; set; }

        public string MethodName { get; set; }

        public List<CParameter> Parameter { get; set; } = new List<CParameter>();
        public string CodeSnippetFile { get; set; }
        public string CodeSnippet { get; set; }
        public COperationIs MethodIs { get; set; }
        public int TextLength {
            get
            {
                //todo: render it to get exact length
                
                var length = MethodName.Length;
                length += ReturnType.Length;
                length += AccessModifier.ToString().Length;

                foreach (var param in Parameter)
                {
                    length += param.Type.Length;
                    length += param.ParameterName.Length;
                    length += 2; // , and spaces
                }

                length += 5;// ( ) and spaces
                return length;
            } }

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitCMethod(this);
        }
    }
}