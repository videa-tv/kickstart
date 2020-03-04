using System.IO;

namespace Kickstart.Pass3.CSharp
{
    internal class SnippetService
    {
        public string GetCodeSnippet(string methodName, string visitMethodSnippetCodeFile)
        {
            if (string.IsNullOrEmpty(visitMethodSnippetCodeFile))
                return null;
            //extract snippet from real code
            var completeFile = File.ReadAllText(visitMethodSnippetCodeFile);
            var startTag = $"//<startsnippet({methodName})>";
            var endTag = $"//</endsnippet({methodName})>";
            var startIndex = completeFile.IndexOf(startTag) + startTag.Length;
            var endIndex = completeFile.IndexOf(endTag);

            var snippent = completeFile.Substring(startIndex, endIndex - startIndex);
            return snippent;
        }
    }
}