using System.Collections.Generic;

namespace Kickstart.GroupService
{
    public interface IFindAndReplaceService
    {
        void FileContentFindAndReplace(string dirName, string find, string replace, bool searchSubDirs, IEnumerable<string> excludeExtensions = null, IEnumerable<string> includeExtensions = null);
    }
}