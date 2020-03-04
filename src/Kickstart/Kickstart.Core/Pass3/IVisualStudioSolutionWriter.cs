using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass3
{
    public interface IVisualStudioSolutionWriter
    {
        //string OutputRootPath { get; set; }

        void Write(string outputRootPath, CSolution solution);
    }
}