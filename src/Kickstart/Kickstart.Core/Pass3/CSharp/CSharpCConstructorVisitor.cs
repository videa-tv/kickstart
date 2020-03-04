using System.Linq;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCConstructorVisitor : ICConstructorVisitor
    {
        private readonly ICodeWriter _codeWriter;

        public CSharpCConstructorVisitor(ICodeWriter codeWriter)
        {
            _codeWriter = codeWriter;
        }

        public string ProjectPath { get; set; }

        public void Visit(IVisitor visitor, CConstructor constructor)
        {
            string staticString = constructor.IsStatic ? "static " : string.Empty;
            var accessModifier = constructor.IsStatic ? string.Empty : $"{constructor.AccessModifier.GetString()} ";
            _codeWriter.Write($"{accessModifier}{staticString}{constructor.ConstructorName}(");

            var first = true;
            foreach (var param in constructor.Parameter)
            {
                if (!first)
                    _codeWriter.Write(", ");

                visitor.VisitCParameter(param);
                first = false;
            }
            _codeWriter.Write(")");

            var passToBaseClass = constructor.Parameter.Where(p => p.PassToBaseClass);

            if (passToBaseClass.Any())
            {
                _codeWriter.Write(" : base(");
                var firstParameter = true;
                foreach (var parameter in passToBaseClass)
                {
                    if (!firstParameter)
                        _codeWriter.Write(", ");
                    firstParameter = false;
                    _codeWriter.Write($"{parameter.ParameterName}");
                }
                _codeWriter.Write(")");
            }

            {
                _codeWriter.WriteLine(string.Empty);
                _codeWriter.WriteLine("{");
                _codeWriter.Indent();
                if (!string.IsNullOrEmpty(constructor.CodeSnippetFile) ||
                    !string.IsNullOrEmpty(constructor.CodeSnippet))
                {
                    if (!string.IsNullOrEmpty(constructor.CodeSnippet))
                        _codeWriter.WriteLine(constructor.CodeSnippet);
                    if (!string.IsNullOrEmpty(constructor.CodeSnippetFile))
                    {
                        var snippetService = new SnippetService();
                        _codeWriter.WriteLine(snippetService.GetCodeSnippet(constructor.ConstructorName,
                            constructor.CodeSnippetFile));
                    }
                }
                else
                {
                    //_codeWriter.WriteLine("throw new NotImplementedException();");
                }
                _codeWriter.Unindent();
                _codeWriter.WriteLine("}");
                _codeWriter.WriteLine();
            }
        }
    }
}