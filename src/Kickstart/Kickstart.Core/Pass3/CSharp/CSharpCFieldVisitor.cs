using System;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCFieldVisitor : ICFieldVisitor
    {
        private readonly ICodeWriter _codeWriter;

        public CSharpCFieldVisitor(ICodeWriter codeWriter)
        {
            _codeWriter = codeWriter;
        }

        public void Visit(CField field)
        {
            if (field.FieldType == null)
                throw new ArgumentException("FieldType not set");
            var readOnly = field.IsReadonly ? "readonly " : string.Empty;
            var isConst = field.IsConst ? "const " : string.Empty;
            var staticTest = field.IsStatic ? "static " : string.Empty;

            if (!string.IsNullOrEmpty(field.Comment))
            {
                _codeWriter.WriteLine("/// <summary>");
                _codeWriter.WriteLine($"///{field.Comment}");
                _codeWriter.WriteLine("/// </summary>");
            }

            _codeWriter.Write(
                $"{field.AccessModifier.GetString()} {staticTest}{readOnly}{isConst}{field.FieldType} {field.FieldName}");

            if (field.DefaultValue != null)
                _codeWriter.Write($" = {field.DefaultValue}");
            _codeWriter.WriteLine(";");
        }
    }
}