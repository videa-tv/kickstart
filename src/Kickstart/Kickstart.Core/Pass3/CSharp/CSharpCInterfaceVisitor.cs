using System.Linq;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCInterfaceVisitor : ICInterfaceVisitor
    {
        public CSharpCInterfaceVisitor(ICodeWriter codeWriter)
        {
            CodeWriter = codeWriter;
        }

        public ICodeWriter CodeWriter { get; }

        public void Visit(IVisitor visitor, CInterface @interface)
        {
            CodeWriter.Clear();
            //CodeWriter.WriteLine("using System;"); //todo: use metadata
            foreach (var r in @interface.NamespaceRef)
                CodeWriter.WriteLine($"using {r.ReferenceTo.NamespaceName};");

            CodeWriter.WriteLine();

            CodeWriter.WriteLine($"namespace {@interface.Namespace}");
            CodeWriter.WriteLine("{");
            CodeWriter.Indent();


            CodeWriter.Write($"public interface {@interface.InterfaceName}");
            if (@interface.IsGeneric)
                CodeWriter.Write("<T>");

            if (@interface.InheritsFrom != null)
            {
                CodeWriter.Write($" : {@interface.InheritsFrom.InterfaceName}");
                if (@interface.InheritsFrom.IsGeneric)
                    CodeWriter.Write("<T>");
            }
            if (@interface.Where.Any())
            {
                CodeWriter.Write(" where ");
                foreach (var w in @interface.Where)
                {
                    CodeWriter.Write(w.WhereName);
                }
            }

            CodeWriter.WriteLine("");
            CodeWriter.WriteLine("{");
            CodeWriter.Indent();
            //CodeWriter.WriteLine("//methods go here");
            int methodIndex = 0;
            foreach (var method in @interface.Method)
            {
                method.Accept(visitor);
                methodIndex++;
                /*
                if (methodIndex < @interface.Method.Count)
                {
                    CodeWriter.WriteLine();
                }*/
            }
            CodeWriter.Unindent();
            CodeWriter.WriteLine("}");
            CodeWriter.Unindent();
            CodeWriter.Write("}");
        }
    }
}