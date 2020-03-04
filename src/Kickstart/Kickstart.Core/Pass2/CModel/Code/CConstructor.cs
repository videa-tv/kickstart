using System.Collections.Generic;
using Kickstart.Interface;

namespace Kickstart.Pass2.CModel.Code
{
    public class CConstructor : CPart
    {
        public CAccessModifier AccessModifier { get; set; } = CAccessModifier.Public;
        public string ConstructorName { get; set; }

        public List<CParameter> Parameter { get; set; } = new List<CParameter>();
        public string CodeSnippetFile { get; set; }
        public string CodeSnippet { get; set; }
        public bool IsStatic { get; internal set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}