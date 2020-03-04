namespace Kickstart.GroupService
{
    public interface IFindAndRenameService
    {
        void DirectoryFindAndRename(string dirName, string find, string replace, bool searchSubDirs);
        void FilesFindAndRename(string dirName, string find, string replace, bool searchSubDirs);
    }
}