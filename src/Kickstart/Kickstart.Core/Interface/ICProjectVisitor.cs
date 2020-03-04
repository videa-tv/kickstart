using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Interface
{
    public interface ICProjectVisitor
    {
        IFileWriter FileWriter { get; }

        //string ProjectPath { get; set; }
        //string RootPath { get; set; }

        void Visit(IVisitor visitor, CProject project);
    }
}