using System.IO;
using System.Reflection;
using System.Text;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass2
{
    public interface ISolutionFileService
    {
        CProject BuildSolutionFilesProject(KSolution mSolution);
    }

    public class SolutionFileService : ISolutionFileService
    {
        public CProject BuildSolutionFilesProject(KSolution mSolution)
        {
            var project = new CProject
            {
                ProjectName = "SolutionFiles",
                ProjectShortName = "SolutionFiles",
                ProjectFolder = string.Empty,
                ProjectType = CProjectType.CsProj,
                ProjectIs = CProjectIs.SolutionFiles
            };
            AddGitIgnoreFile(project);
            AddNugetConfig(project);
            AddRulesetFile(mSolution, project);
            return project;
        }

        private void AddRulesetFile(KSolution mSolution, CProject project)
        {
            var nuGetConfigFile = ReadResourceFile($".ruleset");
            nuGetConfigFile = nuGetConfigFile.Replace("##CompanyName##", mSolution.CompanyName);
            nuGetConfigFile = nuGetConfigFile.Replace("##SolutionName##", mSolution.SolutionName);

            project.ProjectContent.Add(new CProjectContent
            {
                Content = new CText {Text = nuGetConfigFile},
                BuildAction = CBuildAction.None,
                File = new CFile
                {
                    Folder = $@"",
                    FileName = $"{mSolution.CompanyName}.{mSolution.SolutionName}.ruleset",
                    Encoding = Encoding.ASCII
                }
            });
        }

        private void AddNugetConfig(CProject project)
        {
            var nuGetConfigFile = ReadResourceFile("NuGet.config");
            project.ProjectContent.Add(new CProjectContent
            {
                Content = new CText {Text = nuGetConfigFile},
                BuildAction = CBuildAction.None,
                File = new CFile {Folder = $@"", FileName = $"NuGet.config", Encoding = Encoding.ASCII}
            });
        }

        protected void AddGitIgnoreFile(CProject project)
        {
            var gitIgnoreFile = ReadResourceFile(".gitignore");
            project.ProjectContent.Add(new CProjectContent
            {
                Content = new CText {Text = gitIgnoreFile},
                BuildAction = CBuildAction.None,
                File = new CFile {Folder = $@"", FileName = $".gitignore", Encoding = Encoding.ASCII}
            });
        }

        private string ReadResourceFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Kickstart.Core.NetStandard.Boilerplate.{fileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}