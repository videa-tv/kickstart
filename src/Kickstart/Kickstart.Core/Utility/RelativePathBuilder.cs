using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Utility
{
    public class RelativePathBuilder
    {
        public string GetRelativePath(string path1, string path2)
        {


            if (!path2.EndsWith("/"))
                path2 += "/";

            System.Uri uri1 = new Uri(path1);

            var directoryName = Path.GetDirectoryName(path2);
            if (!path2.EndsWith("/"))
                path2 += "/";

            System.Uri uri2 = new Uri(path2);

            return uri1.MakeRelativeUri(uri2).ToString().Replace(@"/", @"\");
        }
    }
}
