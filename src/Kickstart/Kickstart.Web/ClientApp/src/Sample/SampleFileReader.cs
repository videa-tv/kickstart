using System.IO;
using System.Reflection;

namespace Kickstart.Sample
{
    public static class SampleFileReader
    {
        public static string ReadSampleFile(string folderName, string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Kickstart.Vsix.Sample.{folderName}.{fileName}";

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