using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kickstart.GroupService
{
    public class FindAndDeleteService : IFindAndDeleteService
    {
        public void FilesFindAndDelete(string dirName, string find, bool searchSubDirs)
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

            // Get the files in the directory and delete them them.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Name.Contains(find, StringComparison.CurrentCulture))
                {
                    file.Delete();
                }
            }

            if (searchSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    FilesFindAndDelete(subdir.FullName, find, searchSubDirs);
                }
            }
        }

    }
}
