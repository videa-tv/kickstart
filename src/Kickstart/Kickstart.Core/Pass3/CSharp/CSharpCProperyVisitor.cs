using System;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCPropertyVisitor : ICPropertyVisitor
    {
        private readonly ICodeWriter _codeWriter;

        public CSharpCPropertyVisitor(ICodeWriter codeWriter)
        {
            _codeWriter = codeWriter;
        }

        public void Visit(CProperty property)
        {
            if (property.Type == null)
                throw new ArgumentException("Type not set");
            var staticString = property.IsStatic ? "static " : string.Empty;

            _codeWriter.Write(
                $"{property.AccessModifier.GetString()} {staticString}{property.Type} {property.PropertyName}");
            _codeWriter.Write(" {");
            _codeWriter.Write(" get;");
            _codeWriter.Write(" set;");

            if (property.DefaultValue != null)
            {
                _codeWriter.Write(" } ");
                _codeWriter.WriteLine($" = {property.DefaultValue};");
            }
            else
            {
                _codeWriter.WriteLine(" } ");
            }
        }
    }
}