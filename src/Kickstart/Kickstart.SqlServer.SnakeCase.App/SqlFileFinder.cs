using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kickstart.SqlServer.SnakeCase.App
{
    public class SqlFileFinder
    {
        private readonly ISnakeCaseService _snakeCaseService;

        public SqlFileFinder(ISnakeCaseService snakeCaseService)
        {
            _snakeCaseService = snakeCaseService;
        }
        public void FindAndSnakeCaseSqlFiles(string dirName, bool searchSubDirs = true, IEnumerable<string> excludeExtensions = null, IEnumerable<string> includeExtensions =null )
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(dirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + dirName);
            }

            if (dir.Name == ".git")
            {
                return; //todo: pass as parameter
            }
            if (dir.Name == ".vs")
            {
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory 
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (excludeExtensions != null)
                {
                    var excludeIt = false;
                    foreach (var excludeExtension in excludeExtensions)
                    {

                        if (file.Extension.Equals(excludeExtension, StringComparison.CurrentCultureIgnoreCase))
                        {
                            excludeIt = true;
                            break;
                        }
                    }
                    if (excludeIt)
                        continue;

                }

                if (includeExtensions != null)
                {
                    var foundIt = false;
                    foreach (var includeExtension in includeExtensions)
                    {

                        if (file.Extension.Equals(includeExtension, StringComparison.CurrentCultureIgnoreCase))
                        {
                            foundIt = true;
                            break;
                        }
                    }

                    if (!foundIt)
                    {
                        continue;

                    }
                }

                _snakeCaseService.SnakeCaseFile(file.FullName);
            }

            if (searchSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    FindAndSnakeCaseSqlFiles(subdir.FullName, searchSubDirs, excludeExtensions, includeExtensions);
                }
            }
        }
    }
}
