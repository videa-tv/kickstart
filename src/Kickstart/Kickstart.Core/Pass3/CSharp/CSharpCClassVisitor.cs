using System;
using System.Collections.Generic;
using System.Linq;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCClassVisitor : ICClassVisitor
    {
        public CSharpCClassVisitor(ICodeWriter codeWriter)
        {
            CodeWriter = codeWriter;
        }

        public string ProjectPath { get; set; }

        public ICodeWriter CodeWriter { get; }

        public void Visit(IVisitor visitor, CClass cclass)
        {
            if (string.IsNullOrEmpty(cclass.ClassName))
            {
                throw new ApplicationException("ClassName not set on class");
            }
            CodeWriter.Clear();
            var namespaceList = new List<string>();
            //namespaceList.Add("System");
            //namespaceList.Add("System.Collections.Generic");

            //CodeWriter.WriteLine("using System;"); //todo: use metadata
            //CodeWriter.WriteLine("using System.Collections.Generic;"); //todo: use metadata

            namespaceList.Add(cclass.Namespace.NamespaceName);// don't add using if the ref matches the namespace

            if (cclass.NamespaceRef.Any())
            {
                foreach (var r in cclass.NamespaceRef)
                {
                    if (r.ReferenceTo == null)
                        continue;
                    if (namespaceList.Contains(r.ReferenceTo.NamespaceName))
                        continue; //don't list same namespace more than once
                    CodeWriter.Write("using ");
                    if (!string.IsNullOrWhiteSpace(r.ReferenceTo.Alias))
                    {
                        CodeWriter.Write($"{r.ReferenceTo.Alias} = ");
                    }
                    CodeWriter.WriteLine($"{r.ReferenceTo.NamespaceName};");
                    namespaceList.Add(r.ReferenceTo.NamespaceName);
                }
                CodeWriter.WriteLine(string.Empty);
            }

            CodeWriter.WriteLine($"namespace {cclass.Namespace}");
            CodeWriter.WriteLine("{");
            CodeWriter.Indent();

            foreach (var classAttribute in cclass.ClassAttribute)
                classAttribute.Accept(visitor);

            var abstractText = cclass.IsAbstract ? "abstract " : string.Empty;
            var staticText = cclass.IsStatic ? "static " : string.Empty;

            CodeWriter.Write(
                $"{cclass.AccessModifier.GetString() + " "}{staticText}{abstractText}class {cclass.ClassName}");
            if (cclass.InheritsFrom != null || cclass.Implements.Count > 0)
                CodeWriter.Write($" : ");
            if (cclass.InheritsFrom != null)
                CodeWriter.Write($"{cclass.InheritsFrom.ClassName}");
            if (cclass.Implements.Count > 0)
            {
                if (cclass.InheritsFrom != null)
                    CodeWriter.Write(", ");
                var first = true;
                foreach (var @interface in cclass.Implements)
                {
                    if (!first)
                        CodeWriter.Write(", ");

                    CodeWriter.Write($"{@interface.InterfaceName}");
                    if (@interface.IsGeneric)
                        CodeWriter.Write("<T>");
                    first = false;
                }
            }

            if (cclass.Where.Any())
            {
                CodeWriter.Write(" where ");
                foreach (var w in cclass.Where)
                {
                    CodeWriter.Write(w.WhereName);
                }
            }

            CodeWriter.WriteLine("");
            CodeWriter.WriteLine("{");
            CodeWriter.Indent();

            var KickstartRegions = false;

            if (KickstartRegions)
                CodeWriter.WriteLine("#region Fields");

            foreach (var field in cclass.Field)
                field.Accept(visitor);
            if (KickstartRegions)
            {
                CodeWriter.WriteLine("#endregion Fields");

                CodeWriter.WriteLine("#region Properties");
            }
            foreach (var property in cclass.Property)
                property.Accept(visitor);

            //CodeWriter.WriteLine();

            if (KickstartRegions)
            {
                CodeWriter.WriteLine("#endregion Properties");

                CodeWriter.WriteLine("#region Constructors");
            }
            foreach (var constructor in cclass.Constructor)
                constructor.Accept(visitor);
            if (KickstartRegions)
            {
                CodeWriter.WriteLine("#endregion Constructors");
                CodeWriter.WriteLine("#region Methods");
            }
            int methodIndex = 0;
            foreach (var method in cclass.Method)
            {
                method.Accept(visitor);
                methodIndex++;
                if (methodIndex < cclass.Method.Count)
                {
                    CodeWriter.WriteLine();
                }
            }
            if (KickstartRegions)
                CodeWriter.WriteLine("#endregion Methods");
            CodeWriter.Unindent();
            CodeWriter.WriteLine("}");
            CodeWriter.Unindent();
            CodeWriter.Write("}");
        }
    }
}