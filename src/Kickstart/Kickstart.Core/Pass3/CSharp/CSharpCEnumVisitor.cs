using System;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCEnumVisitor : ICEnumVisitor
    {
        public CSharpCEnumVisitor(ICodeWriter codeWriter)
        {
            CodeWriter = codeWriter;
        }
        public ICodeWriter CodeWriter { get; }
        public void Visit(IVisitor visitor, CEnum cenum)
        {
            CodeWriter.WriteLine($"namespace {cenum.Namespace}");
            CodeWriter.WriteLine("{");
            CodeWriter.Indent();

            CodeWriter.WriteLine($"public enum {cenum.EnumName}");
            CodeWriter.WriteLine("{");
            CodeWriter.Indent();
            bool first = true;
            foreach (var v in cenum.EnumValues)
            {
                if (!first)
                    CodeWriter.WriteLine(",");

                CodeWriter.Write($"{v.EnumValueName} = {v.EnumValue}");
                
                first = false;
            }
            CodeWriter.WriteLine();
            CodeWriter.Unindent();
            CodeWriter.WriteLine("}");

            CodeWriter.Unindent();
            CodeWriter.WriteLine("}");


        }
    }
}