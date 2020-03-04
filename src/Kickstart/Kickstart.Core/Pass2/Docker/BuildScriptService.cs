using System.IO;
using System.Reflection;
using System.Text;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.GrpcServiceProject;

namespace Kickstart.Pass2.Docker
{
    public class BuildScriptService : IBuildScriptService
    {
        private KDockerBuildScriptProject _mDockerBuildScriptProject;
        private readonly IGrpcPortService _grpcPortService;

        public BuildScriptService(IGrpcPortService grpcPortService)
        {
            _grpcPortService = grpcPortService;

        }
        public CProject Execute(string solutionName, KDockerBuildScriptProject mDockerBuildScriptProject)
        {
            _mDockerBuildScriptProject = mDockerBuildScriptProject;
            var project = new CProject
            {
                ProjectName = mDockerBuildScriptProject.ProjectFullName,
                ProjectShortName = mDockerBuildScriptProject.ProjectShortName,
                ProjectFolder = "build", // mDockerBuildScriptProject.ProjectFolder,
                ProjectType = CProjectType.CsProj,
                ProjectIs = CProjectIs.DockerBuildScripts
            };
            AddFile(solutionName, project, "build-apps.sh");
            //AddFile(project, "build-output.sh");
            //AddFile(project, "docker-image.sh");
            AddFile(solutionName, project, "service.json2", $"{mDockerBuildScriptProject.ProjectName.ToLower()}service.json");
            //AddFile(project, "web.json", $"{mDockerBuildScriptProject.ProjectName.ToLower()}web.json");
            AddFile(solutionName, project, "run-build-apps.sh");
            //AddFile(project, "run-build-output.sh");
            //AddFile(project, "run-docker-image.sh");
            AddFile(solutionName, project, "run-test-apps.sh");
            AddFile(solutionName, project, "test-apps.sh");
            AddFile(solutionName, project, "set-vars.sh");

            return project;
        }

        protected void AddFile(string solutionName, CProject project, string fileNameIn, string fileNameOut = null)
        {
            if (fileNameOut == null)
                fileNameOut = fileNameIn;

            var text = new CText();
            var textTemplate = ReadResourceFile(fileNameIn);
            textTemplate = textTemplate.Replace("##CompanyName##", $"{_mDockerBuildScriptProject.CompanyName}");
            textTemplate =
                textTemplate.Replace("##COMPANYNAME##", $"{_mDockerBuildScriptProject.CompanyName.ToUpper()}");
            textTemplate =
                textTemplate.Replace("##companyname##", $"{_mDockerBuildScriptProject.CompanyName.ToLower()}");

            textTemplate = textTemplate.Replace("##ProjectName##", $"{_mDockerBuildScriptProject.ProjectName}");
            textTemplate =
                textTemplate.Replace("##PROJECTNAME##", $"{_mDockerBuildScriptProject.ProjectName.ToUpper()}");
            textTemplate =
                textTemplate.Replace("##projectname##", $"{_mDockerBuildScriptProject.ProjectName.ToLower()}");

            textTemplate = textTemplate.Replace("##ProjectSuffix##", $"{_mDockerBuildScriptProject.ProjectSuffix}");
            textTemplate =
                textTemplate.Replace("##PROJECTSUFFIX##", $"{_mDockerBuildScriptProject.ProjectSuffix.ToUpper()}");
            textTemplate =
                textTemplate.Replace("##projectsuffix##", $"{_mDockerBuildScriptProject.ProjectSuffix.ToLower()}");

            int port = _grpcPortService.GeneratePortNumber(solutionName);
            textTemplate =
                textTemplate.Replace("##PORT##", $"{port}");

            text.Text = textTemplate;
            //text.Text = text.Text.Replace("NamespacePlaceholder", $"{_grpcKProject.CompanyName}.{_grpcKProject.ProjectName}{_grpcKProject.NamespaceSuffix}.{_grpcKProject.ProjectSuffix}");
            project.ProjectContent.Add(new CProjectContent
            {
                Content = text,
                BuildAction = CBuildAction.None,
                File = new CFile {Folder = $@"", FileName = fileNameOut, Encoding = Encoding.ASCII}
            });
        }

        private string ReadResourceFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Kickstart.Core.NetStandard.Boilerplate.Build.{fileName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}