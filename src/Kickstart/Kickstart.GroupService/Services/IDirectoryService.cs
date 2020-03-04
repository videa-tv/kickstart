namespace Kickstart.GroupService
{
    public interface IDirectoryService
    {
        void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs);
    }
}