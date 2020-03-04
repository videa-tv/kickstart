namespace Kickstart.GroupService
{
    public interface IFindAndDeleteService
    {
        void FilesFindAndDelete(string dirName, string find, bool searchSubDirs);
    }
}