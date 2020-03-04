using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Kickstart.Interface;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Utility;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Logging;

namespace Kickstart.Pass3.VisualStudio2017
{
    public class CProjectVisitor : ICProjectVisitor
    {
        private readonly ICodeWriter _codeWriter;
        private readonly ILogger _logger;

        private readonly INugetQueryService _nugetQueryService;
        
        public CProjectVisitor(ILogger<CProjectVisitor> logger, ICodeWriter codeWriter, IFileWriter fileWriter, INugetQueryService nugetQueryService)
        {
            _logger = logger;
            _codeWriter = codeWriter;
            FileWriter = fileWriter;
            _nugetQueryService = nugetQueryService;
        }

        public IFileWriter FileWriter { get; }

        public void Visit(IVisitor visitor, CProject project)
        {
            _logger.LogInformation($"Visiting {GetType()}, Project: {project.ProjectName}");

            //<startsnippet(Visit)>
            _codeWriter.Clear();
            if (FileWriter != null)
                if (!string.IsNullOrEmpty(project.ProjectFolder))
                    FileWriter.CurrentPath = Path.Combine(FileWriter.RootPath, project.ProjectFolder);
            var projectRootElement = GetProjectRootElement(project);
            projectRootElement.DefaultTargets = "Build";
            projectRootElement.ToolsVersion = "4.0";


            if (!string.IsNullOrEmpty(project.StartupObject))
            {
                var propertyGroup = projectRootElement.AddPropertyGroup();

                propertyGroup.AddProperty("StartupObject", project.StartupObject);
            }
            if (project.ProjectIs.HasFlag(CProjectIs.DataBase))
            {
                var propertyGroup = projectRootElement.AddPropertyGroup();

                propertyGroup.AddProperty("TargetDatabase", project.ProjectShortName);
            }


            var refGroup = projectRootElement.AddItemGroup();

            var added = new List<string>();
            foreach (var reference in project.ProjectReference)
            {
                if (reference == null)
                    continue;

                if (added.Contains(reference.ProjectName))
                    continue;

                var metadata = new List<KeyValuePair<string, string>>();
                //metadata.Add(new KeyValuePair<string, string>("Project", "{" + $"{reference.ProjectGuid}" + "}"));
               // metadata.Add(new KeyValuePair<string, string>("Name", reference.ProjectName));

                //var pathTemp = reference.ProjectFolder.Replace("Code\\", "..\\");//todo: fix hard coding
                var pathTemp = reference.ProjectFolder.Replace("src\\", "..\\..\\src\\"); //todo: fix hard coding
                pathTemp = pathTemp.Replace("test\\", "..\\..\\test\\"); //todo: fix hard coding
                pathTemp = pathTemp.Replace("build\\", "..\\..\\build\\"); //todo: fix hard coding
                var path = Path.Combine(pathTemp, reference.FileName);
                refGroup.AddItem("ProjectReference", path, metadata);

                added.Add(reference.ProjectName);
            }


            //var projectPath = System.IO.Path.Combine(RootPath, project.ProjectName);

            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            //projectRootElement.Save(System.IO.Path.Combine(projectPath, $"{project.ProjectName}.csproj"));

            foreach (var projectContent in project.ProjectContent)
            {

                if (projectRootElement != null)
                    if (projectContent.BuildAction == CBuildAction.Compile)
                    {
                        if (projectRootElement.Items.FirstOrDefault(i =>
                                i.ItemType == "Compile" && i.Include == projectContent.File.Path) == null)
                            projectRootElement.AddItem("Compile", projectContent.File.Path);
                    }
                    else if (projectContent.BuildAction == CBuildAction.Build)
                    {
                        if (projectRootElement.Items.FirstOrDefault(i =>
                                i.ItemType == "Build" && i.Include == projectContent.File.Path) == null)
                            projectRootElement.AddItem("Build", projectContent.File.Path);
                    }
                    else if (projectContent.BuildAction == CBuildAction.None)
                    {
                        if (projectRootElement.Items.FirstOrDefault(i =>
                                i.ItemType == "None" && i.Include == projectContent.File.Path) == null)
                        {
                            var ii = projectRootElement.AddItem("None", projectContent.File.Path);
                            if (projectContent.CopyToOutputDirectory)
                                ii.AddMetadata("CopyToOutputDirectory", "Always");
                            if (projectContent.CopyToPublishDirectory)
                                ii.AddMetadata("CopyToPublishDirectory", "PreserveNewest");
                        }
                    }
                    else if (projectContent.BuildAction == CBuildAction.PostDeploy)
                    {
                        if (projectRootElement.Items.FirstOrDefault(i =>
                                i.ItemType == "PostDeploy" && i.Include == projectContent.File.Path) == null)
                            projectRootElement.AddItem("PostDeploy", projectContent.File.Path);
                    }
                    else if (projectContent.BuildAction == CBuildAction.DoNotInclude)
                    {
                    }
                    else if (projectContent.BuildAction == CBuildAction.EmbeddedResource)
                    {
                        if (projectRootElement.Items.FirstOrDefault(i =>
                               i.ItemType == "EmbeddedResource" && i.Include == projectContent.File.Path) == null)
                            projectRootElement.AddItem("EmbeddedResource", projectContent.File.Path);
                    }

                    else
                    {
                        throw new NotImplementedException(nameof(projectContent.BuildAction));
                    }
            }
            var nuGetRefGroup = projectRootElement.AddItemGroup();

            foreach (var nuGetReference in project.NuGetReference)
            {   
                var pie = nuGetRefGroup.AddItem("PackageReference", nuGetReference.NuGetName);

                var nugetVersion = _nugetQueryService.GetLatestVersionNumber(nuGetReference.NuGetName, nuGetReference.Version);
                pie.AddMetadata("Version", nugetVersion /* nuGetReference.Version*/, true);
            }

            //make it compile the proto
            var protoCompileGroup = projectRootElement.AddItemGroup();
            var protobufItem = protoCompileGroup.AddItem("Protobuf", "Proto/*.proto");
            protobufItem.AddMetadata("ProtoRoot", "Proto");
            protobufItem.AddMetadata("GrpcServices", "Both");
            protobufItem.AddMetadata("CompileOutputs", "true");


            //add post build step to build NuGet
            if (project.PostBuildEventStep.Count > 0)
            {
                var propertyGroupPostBuild = projectRootElement.AddPropertyGroup();
                var stringBuilder = new StringBuilder();
                foreach (var postBuildEventStep in project.PostBuildEventStep)
                    stringBuilder.AppendLine(postBuildEventStep.Value);
                propertyGroupPostBuild.AddProperty("PostBuildEvent", stringBuilder.ToString());
            }
            projectRootElement.Save(stringWriter);
            projectRootElement = null;
        
            if (!project.ProjectIs.HasFlag(CProjectIs.DockerBuildScripts) &&
                !project.ProjectIs.HasFlag(CProjectIs.SolutionFiles))
            {
                _codeWriter.Write(sb.ToString());

                if (FileWriter != null)
                {
                    if (!string.IsNullOrEmpty(project.ProjectFolder))
                        FileWriter.CurrentPath = Path.Combine(FileWriter.RootPath, project.ProjectFolder);
                    var projectText = _codeWriter.ToString();
                    if (project.ProjectIs.HasFlag(CProjectIs.DataBase))
                    {
                        projectText = projectText.Replace("$(MSBuildExtensionsPath)",
                             @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild");
                    }
                    if (project.ProjectIs.HasFlag(CProjectIs.DockerCompose))
                    {
                        //fixup error when adding project to .sln
                        //projectText = projectText.Replace(@"Sdk=""Microsoft.Docker.Sdk""",
                       //this gets down when the project is added to .sln, now.  //    @"Sdk=""""");

                    }
                    FileWriter.WriteFile(project.FileName, projectText);
                    project.Path = FileWriter.FilePath;
                    visitor.AddProjectToSolution(project, FileWriter.FilePath);

                    if (project.ProjectIs.HasFlag(CProjectIs.DockerCompose))
                    {
                        //rewrite the original file, now that its been added to the .sln
                        projectText = _codeWriter.ToString();
                        FileWriter.WriteFile(project.FileName, projectText, overwriteExisting:true);

                    }
                }
            }
            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();
            foreach (var projectContent in project.ProjectContent)
            {
                _codeWriter.Clear();
                projectContent.Accept(visitor);
            }

            //var projectPath = System.IO.Path.Combine(RootPath, project.ProjectName);

            //projectRootElement.Save(System.IO.Path.Combine(projectPath, $"{project.ProjectName}.csproj"));

            // ProjectPath = projectPath;
            //return projectRootElement;
            //_projects.Add(CurrentProjectRootElement);

            //</endsnippet(Visit)>
            _logger.LogInformation($"Visited {GetType()}, Project: {project.ProjectName}");
        }

        private ProjectRootElement GetProjectRootElement(CProject project)
        {
            if (!string.IsNullOrEmpty(project.TemplateProjectPath))
            {

                if (!string.IsNullOrEmpty(project.TemplateProjectZip))
                {
                    //unzip it first
                    var zipPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), project.TemplateProjectZip);
                    
                    var unZipPath = Path.GetDirectoryName(zipPath);
                    unZipPath = Path.Combine(unZipPath, "unzipped");
                    if (Directory.Exists(unZipPath))
                    {
                        Directory.Delete(unZipPath, true);
                    }

                    System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, unZipPath);
                }
                var fullPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), project.TemplateProjectPath);

                var projectRootElement = ProjectRootElement.Open(fullPath);
                var assemblyName = projectRootElement.Properties.FirstOrDefault(p => p.Name == "AssemblyName");
                if (assemblyName != null)
                    assemblyName.Value = project?.AssemblyName;
                if (project.DefaultNamespace != null)
                {
                    var rootNamespace = projectRootElement.Properties.FirstOrDefault(p => p.Name == "RootNamespace");

                    if (rootNamespace != null)
                        rootNamespace.Value = project?.DefaultNamespace;
                }

                var projectGuid = projectRootElement.Properties.FirstOrDefault(p => p.Name == "ProjectGuid");
                if (projectGuid != null)
                    projectGuid.Value = project.ProjectGuid.ToString();

                return projectRootElement;
            }
            else
            {
                var projectRootElement = ProjectRootElement.Create();
                return projectRootElement;
            }
        }
    }
}