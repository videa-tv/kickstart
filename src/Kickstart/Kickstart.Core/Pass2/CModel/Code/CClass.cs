using System;
using System.Collections.Generic;
using Kickstart.Interface;
using Newtonsoft.Json;

namespace Kickstart.Pass2.CModel.Code
{
    public class CClass : CPart
    {
        public List<CNamespaceRef> NamespaceRef { get; set; } = new List<CNamespaceRef>();

        public List<CClassAttribute> ClassAttribute { get; set; } = new List<CClassAttribute>();
        public CAccessModifier AccessModifier { get; set; } = CAccessModifier.Public;
        public bool IsAbstract { get; set; }
        public CNamespace Namespace { get; set; }
        public bool IsStatic { get; set; }
        private string _className;
        public string ClassName
        {
            get
            {
                if (string.IsNullOrEmpty(_className))
                {
                    int x = 1;
                }
                return _className;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ApplicationException("Cannot set ClassName to null or empty");
                }
                else
                {
                    _className = value;
                }
            }
        }
        public List<CConstructor> Constructor { get; set; } = new List<CConstructor>();

        public List<CMethod> Method { get; set; } = new List<CMethod>();
        public List<CProperty> Property { get; set; } = new List<CProperty>();
        public List<CField> Field { get; set; } = new List<CField>();
        public CClass InheritsFrom { get; set; }
        public List<CInterface> Implements { get; set; } = new List<CInterface>();

        [JsonIgnore]
        public string ClassNameAsCamelCase => char.ToLowerInvariant(ClassName[0]) + ClassName.Substring(1);

        public IList<CWhere> Where { get; set; } = new List<CWhere>();

        public CClass(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                throw new ApplicationException("ClassName cannot be empty");
            }
            _className = className;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.VisitCClass(this);
        }
    }
}