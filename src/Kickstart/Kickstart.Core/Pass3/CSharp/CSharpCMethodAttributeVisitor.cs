using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCMethodAttributeVisitor : ICMethodAttributeVisitor
    {
        private readonly ICodeWriter _codeWriter;

        public CSharpCMethodAttributeVisitor(ICodeWriter codeWriter)
        {
            _codeWriter = codeWriter;
        }

        public void Visit(IVisitor visitor, CMethodAttribute field)
        {
            _codeWriter.WriteLine($"[{field.AttributeName}]");
        }
    }
}