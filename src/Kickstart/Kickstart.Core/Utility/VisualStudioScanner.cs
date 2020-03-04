using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Utility
{
    public class VisualStudioScanner
    {
        public IEnumerable<string> ScanForSolutions(string path)
        {
            return Directory.EnumerateFiles(path, "*.sln", SearchOption.AllDirectories);
        }
    }
}
