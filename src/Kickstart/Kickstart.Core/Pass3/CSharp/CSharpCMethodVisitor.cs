using System;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCMethodVisitor : ICMethodVisitor
    {
        private readonly ICodeWriter _codeWriter;

        public CSharpCMethodVisitor(ICodeWriter codeWriter)
        {
            _codeWriter = codeWriter;
        }

        public string ProjectPath { get; set; }

        public void Visit(IVisitor visitor, CMethod method)
        {
            NewMethod(method);

            foreach (var methodAttribute in method.Attribute)
                methodAttribute.Accept(visitor);

            var modifier = !method.SignatureOnly ? method.AccessModifier.GetString() + " " : string.Empty;
            var abstractText = method.IsAbstract ? "abstract " : string.Empty;
            var overrideText = method.IsOverride ? "override " : string.Empty;
            var @static = method.IsStatic ? "static " : string.Empty;
            var asyncText = method.IsAsync ? "async " : string.Empty;
            var thisText = method.IsExtensionMethod ? "this " : string.Empty;

            _codeWriter.Write(
                $"{modifier}{@static}{abstractText}{overrideText}{asyncText}{method.ReturnType} {method.MethodName}({thisText}");

            var first = true;
            var paramIndex = 0;
            _codeWriter.SaveIndent();

            var methodTextLength = method.TextLength;
            foreach (var param in method.Parameter)
            {
                if (!first)
                {   
                        _codeWriter.Write(", ");
                }
                if (methodTextLength > 100 && method.Parameter.Count > 1) //attempt to Mirror Resharper's rules
                {
                    //wrap the parameters to next line
                    _codeWriter.WriteLine("");
                    if (first)
                        _codeWriter.Indent();
                }
                visitor.VisitCParameter(param);
                first = false;
                paramIndex++;
            }
            _codeWriter.Write(")");
            _codeWriter.RestoreIndent();
            if (method.SignatureOnly || method.IsAbstract)
            {
                _codeWriter.WriteLine(";");
            }
            else if (method.UseExpressionDefinition)
            {
                _codeWriter.WriteLine($" => {method.CodeSnippet}");
            }
            else
            {
                _codeWriter.WriteLine(string.Empty);
                _codeWriter.WriteLine("{");
                _codeWriter.SaveIndent();
                _codeWriter.Indent();
                if (!string.IsNullOrEmpty(method.CodeSnippetFile) || !string.IsNullOrEmpty(method.CodeSnippet))
                {
                    if (!string.IsNullOrEmpty(method.CodeSnippet))
                    {
                        _codeWriter.WriteLine(method.CodeSnippet);
                    }
                    if (!string.IsNullOrEmpty(method.CodeSnippetFile))
                    {
                        var snippetService = new SnippetService();
                        _codeWriter.WriteLine(snippetService.GetCodeSnippet(method.MethodName, method.CodeSnippetFile));
                    }
                }
                else if (method.ReturnType != "void")
                {
                    _codeWriter.WriteLine("throw new NotImplementedException();");
                }
                _codeWriter.Unindent();
                _codeWriter.RestoreIndent();
                _codeWriter.WriteLine("}");
               // _codeWriter.WriteLine();
            }
        }

        private static void NewMethod(CMethod method)
        {
            if (method.ReturnType == null)
                throw new ArgumentException("ReturnType not set");
        }
    }
}