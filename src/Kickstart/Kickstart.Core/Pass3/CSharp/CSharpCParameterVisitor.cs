using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCParameterVisitor : ICParameterVisitor
    {
        private readonly ICodeWriter _codeWriter;

        public CSharpCParameterVisitor(ICodeWriter codeWriter)
        {
            _codeWriter = codeWriter;
        }

        public string ProjectPath { get; set; }

        public void Visit(CParameter parameter)
        {
            _codeWriter.Write($"{parameter.Type} {parameter.ParameterName}");
            if (!string.IsNullOrEmpty(parameter.DefaultValue))
            {
                _codeWriter.Write($" = {parameter.DefaultValue}");
            }
        }
    }
}