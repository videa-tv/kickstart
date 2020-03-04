using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kickstart.GroupService
{
    public class FindAndRenameService : IFindAndRenameService
    {
        public  void DirectoryFindAndRename(string dirName, string find, string replace, bool searchSubDirs)
        {

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(dirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + dirName);
            }

            if (dir.Name.Contains(find))
            {
                string newDirName = dir.FullName.Replace(find, replace, StringComparison.CurrentCulture);

                Directory.Move(dir.FullName, newDirName);

                dir = new DirectoryInfo(newDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();


            if (searchSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    DirectoryFindAndRename(subdir.FullName, find, replace, searchSubDirs);
                }
            }
        }

        public void FilesFindAndRename(string dirName, string find, string replace, bool searchSubDirs)
        {

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(dirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + dirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory and rename them them.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Name.Contains(find, StringComparison.CurrentCulture))
                {
                    var newFileName = file.FullName.Replace(find, replace);
                    file.MoveTo(newFileName);
                }
            }

            if (searchSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    FilesFindAndRename(subdir.FullName, find, replace, searchSubDirs);
                }
            }
        }

    }
}
