using System.IO;
using System.Reflection;

namespace Kickstart.Pass1.Service
{
    public static class ResourceFileReader
    {
        public static string ReadResourceFile(string folderName, string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Kickstart.InputMetadata.{folderName}.{fileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return string.Empty;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}