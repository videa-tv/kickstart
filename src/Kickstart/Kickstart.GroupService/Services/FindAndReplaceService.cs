using Kickstart.TemplateConverterService.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Kickstart.GroupService
{
    public class FindAndReplaceService : IFindAndReplaceService
    {
        public void FileContentFindAndReplace(string dirName, string find, string replace, bool searchSubDirs, IEnumerable<string> excludeExtensions = null, IEnumerable<string> includeExtensions = null)
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

            //using (var progress = new ProgressBar())
            {

                var fileCount = 0;
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

                    string text = File.ReadAllText(file.FullName);
                    text = text.Replace(find, replace, StringComparison.CurrentCulture);
                    File.WriteAllText(file.FullName, text);
                }
                fileCount++;
                //progress.Report((double)fileCount / files.Count());

                if (searchSubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        FileContentFindAndReplace(subdir.FullName, find, replace, searchSubDirs);
                    }
                }
            }
        }

    }
}
