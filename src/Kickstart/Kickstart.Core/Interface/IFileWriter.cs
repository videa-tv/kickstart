using System.Text;

namespace Kickstart.Interface
{
    public interface IFileWriter
    {
        string RootPath { get; set; }
        string CurrentPath { get; set; }
        string FilePath { get; }
        string FileName { get; }
        void WriteFile(string fileName, string content, bool overwriteExisting = false);
        void WriteFile(string fileName, string content, Encoding encoding, bool overwriteExisting = false);
    }
}