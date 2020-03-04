using Kickstart.Pass1.KModel;

namespace Kickstart.Pass2
{
    public interface IKSolutionToCSolutionConverter
    {
        string ConnectionString { get; set; }

        void Convert(KSolution kSolution);
    }
}