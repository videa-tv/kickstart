namespace Kickstart.Interface
{
    public interface ICodeWriter
    {
        void Indent();
        void WriteLine(string v);
        void Unindent();
        void Write(string v);
        void Clear();
        void WriteLine();
        void RestoreIndent();
        void SaveIndent();
    }
}