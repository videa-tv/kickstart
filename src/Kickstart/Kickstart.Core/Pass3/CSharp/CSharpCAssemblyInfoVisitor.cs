using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3.CSharp
{
    public class CSharpCAssemblyInfoVisitor : ICAssemblyInfoVisitor
    {
        private readonly ICodeWriter _codeWriter;

        public CSharpCAssemblyInfoVisitor(ICodeWriter codeWriter)
        {
            _codeWriter = codeWriter;
        }

        public void Visit(IVisitor visitor, CAssemblyInfo assemblyInfo)
        {
            _codeWriter.WriteLine($"using System.Reflection;");
            _codeWriter.WriteLine($"using System.Runtime.InteropServices;");

            _codeWriter.WriteLine($@"[assembly: AssemblyCompany(""{assemblyInfo.AssemblyCompany}"")]");
            _codeWriter.WriteLine($@"[assembly: AssemblyVersion(""{assemblyInfo.AssemblyVersion}"")]");
        }
    }
}