using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCClassAttributeVisitor : ICClassAttributeVisitor
    {
        private readonly ICodeWriter _codeWriter;

        public CSharpCClassAttributeVisitor(ICodeWriter codeWriter)
        {
            _codeWriter = codeWriter;
        }

        public void Visit(IVisitor visitor, CClassAttribute field)
        {
            _codeWriter.WriteLine($"[{field.AttributeName}]");
        }
    }
}