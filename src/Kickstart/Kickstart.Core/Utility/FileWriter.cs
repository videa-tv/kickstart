using System;
using System.IO;
using System.Text;
using Kickstart.Interface;

namespace Kickstart.Utility
{
    public class FileWriter : IFileWriter
    {
        public FileWriter()
        {
            int x = 1;
        }
        public FileWriter(string rootPath)
        {
            RootPath = rootPath;
            CurrentPath = RootPath;
        }

        public string RootPath { get; set; }
        public string CurrentPath { get; set; }
        public string FileName { get; private set; }

        public string FilePath => Path.Combine(CurrentPath, FileName);

        public void WriteFile(string fileName, string content, bool overwriteExisting = false)
        {
            WriteFile(fileName, content, Encoding.UTF8, overwriteExisting);
        }

        public void WriteFile(string fileName, string content, Encoding encoding, bool overwriteExisting = false)
        {
            
            FileName = fileName;
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            if (!overwriteExisting)
            {
                if (File.Exists(FilePath))
                {
                    throw new ApplicationException($"File {FilePath} already exists!");
                }
            }

            File.WriteAllText(FilePath, content, encoding);
            }
    }
}