using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Kickstart.Interface;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Utility;

namespace Kickstart.Pass3.VisualStudio2017
{
    public class FastSolutionVisitor : ICSolutionVisitor, IFastSolutionVisitor
    {                                                                                                
        private readonly ICodeWriter _codeWriter;
        private readonly IFileWriter _fileWriter;

        private CSolution _currentSolution;
        private IVisualStudioSolutionWriter _visualStudioSolutionWriter;

        public FastSolutionVisitor(ICodeWriter codeWriter, IFileWriter fileWriter)
        {
            _codeWriter = codeWriter;
            _fileWriter = fileWriter;
        }

        public void AddProjectToSolution(CProject project, string filePathIn)
        {
            var filePath = filePathIn;
            if (project.ProjectIs.HasFlag(CProjectIs.DockerCompose))
            {
                //dotnet command doesn't like .dcproj
                //make a copy of the file, rename .dcproj to .csproj
                var projectText = File.ReadAllText(filePathIn);

                filePath = filePathIn + ".csproj";
                projectText = projectText.Replace("Microsoft.Docker.Sdk", "Microsoft.NET.Sdk");
                File.WriteAllText(filePath, projectText);
                
                int x = 1;
            }

            var slnFileName = Path.Combine(_fileWriter.RootPath, $"{_currentSolution.SolutionName}.sln");

            CommandProcessor.ExecuteCommand($@"dotnet sln ""{slnFileName}"" add ""{filePath}""", _fileWriter.RootPath);

            if (project.ProjectIs.HasFlag(CProjectIs.DataBase))
            {
                //fixup the project. turn the SqlProj back into true SqlProj
                var solutionFileText = File.ReadAllText(slnFileName);

                solutionFileText = solutionFileText.Replace(
                    $@"Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{project.ProjectName}""",
                    $@"Project(""{{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}}"") = ""{project.ProjectName}""");
                File.WriteAllText(slnFileName, solutionFileText);
            }
            else if (project.ProjectIs.HasFlag(CProjectIs.DockerCompose))
            {
                File.Delete(filePath);
                //fixup the project. turn the "dcProj" back into true Docker compose project
                var solutionFileText = File.ReadAllText(slnFileName);
                solutionFileText = solutionFileText.Replace(".dcproj\"", "\"");
                solutionFileText = solutionFileText.Replace(".dcproj.csproj", ".dcproj");

                solutionFileText = solutionFileText.Replace(
                    $@"Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{project.ProjectName}""",
                    $@"Project(""{{E53339B2-1760-4266-BCC7-CA923CBCF16C}}"") = ""{project.ProjectName}""");
                
                File.WriteAllText(slnFileName, solutionFileText);
            }
        
        }

        public void Visit(IVisitor visitor, CSolution solution)
        {
            _currentSolution = solution;
            Directory.CreateDirectory(_fileWriter.RootPath);
            solution.SolutionPath = Path.Combine(_fileWriter.RootPath, $"{solution.SolutionName}.sln");
            CommandProcessor.ExecuteCommand($"dotnet new sln --name {solution.SolutionName} ", _fileWriter.RootPath);
            foreach (var project in solution.Project)
            {
                if (string.IsNullOrEmpty(project.ProjectFolder))
                    _fileWriter.CurrentPath = _fileWriter.RootPath;
                project.Accept(visitor);
            }
        }

    

        public void AddProjectsToApplication(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                {
                    //var codeWriter = new VisualStudioSolutionWriter

                    var outputRootPath2 = Path.Combine(outputRootPath, solution.SolutionName);
                    
                    if (solution is KApplicationSolution)
                    {
                        var appSolution = solution as KApplicationSolution;
                        
                        foreach (var childSolution in appSolution.ChildSolution)
                        {
                            //codeWriterChild.OutputRootPath =
                             //   Path.Combine(codeWriter.OutputRootPath, childSolution.SolutionName);


                            var solutionPath = Path.Combine(outputRootPath,
                                $"{appSolution.GeneratedSolution.SolutionName}.sln");
                            //add projects to sln
                            foreach (var project in childSolution.GeneratedSolution.Project)
                            {
                                // var relativePath = BuildRelativePath(codeWriter.OutputRootPath,  );
                                CommandProcessor.ExecuteCommand(
                                    $@"dotnet sln ""{solution.GeneratedSolution.SolutionName}.sln"" add ""{childSolution.SolutionName}\\{project.ProjectFolder}\\{project.FileName}""",
                                    solutionPath);

                                if (project.ProjectIs.HasFlag(CProjectIs.DataBase))
                                {
                                    //fixup the project. turn the SqlProj back into true SqlProj
                                    var solutionFileText = File.ReadAllText(solutionPath);

                                    solutionFileText = solutionFileText.Replace(
                                        $@"Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{
                                                project.ProjectName
                                            }""",
                                        $@"Project(""{{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}}"") = ""{
                                                project.ProjectName
                                            }""");
                                    File.WriteAllText(solutionPath, solutionFileText);
                                }
                            }
                        }
                    }
                }
            
        }

        public void AddAllProjectsToMasterSln(List<KSolutionGroup> solutionGroupList, string outputRootPath, string solutionName)
        {
            foreach (var solutionGroup in solutionGroupList)
            {
                AddAllProjectsToMasterSln(solutionGroup, outputRootPath, solutionName);
            }

        }
        public void AddAllProjectsToMasterSln(KSolutionGroup solutionGroup, string outputRootPath, string solutionName)
        {
            foreach (var solution in solutionGroup.Solution)
            {
                AddAllProjectsToMasterSln(solution, outputRootPath, solutionName);
            }


        }
        public void AddAllProjectsToMasterSln(KSolution solution, string outputRootPath, string solutionName)
        {
            AddAllProjectsToMasterSln(solution.GeneratedSolution, outputRootPath, solutionName);
        }
        public void AddAllProjectsToMasterSln(CSolution solution, string outputRootPath, string solutionName)
        {

            var masterSolutionPath = Path.Combine(outputRootPath, $"{solutionName}.sln");
            CommandProcessor.ExecuteCommand($"dotnet new sln  --name {solutionName}", outputRootPath);
           
            foreach (var project in solution.Project)
            {
                AddProjectToSln(outputRootPath, solutionName, masterSolutionPath, project);
            }

        }

        public void AddProjectToSln(string outputRootPath, string solutionName, 
            string masterSolutionPath, CProject project)
        {
            var tempFileName = Path.GetFileName(Path.GetTempFileName());

            var relativePathBuilder = new RelativePathBuilder();

            var tempFolder = relativePathBuilder.GetRelativePath(masterSolutionPath, Path.GetDirectoryName(project.Path));
            var tempRelativeFile = Path.Combine(tempFolder, tempFileName).Replace(@"/", @"\");
            var tempAbsFile = Path.Combine(Path.GetDirectoryName(project.Path), tempFileName);

            if (!Directory.Exists(Path.GetDirectoryName(tempAbsFile)))
            {
                return;
            }

            var projectRelPath = relativePathBuilder.GetRelativePath(masterSolutionPath, Path.GetDirectoryName(project.Path));
            projectRelPath = Path.Combine(projectRelPath, Path.GetFileName(project.Path)).Replace(@"/", @"\");


            var slnTextA = File.ReadAllText(masterSolutionPath);
            if (slnTextA.Contains(projectRelPath))
            {
                return;
            }

            File.WriteAllText(tempAbsFile, "<Project/>");
            //add the mock project
            CommandProcessor.ExecuteCommand($@"dotnet sln ""{solutionName}.sln"" add ""{tempRelativeFile}""", outputRootPath);
            File.Delete(tempAbsFile);

            //update the .sln to point to real project. This is super fast, and won't get errors
            var slnText = File.ReadAllText(masterSolutionPath);
            slnText = slnText.Replace(tempRelativeFile, projectRelPath);
            slnText = slnText.Replace(Path.GetFileNameWithoutExtension(tempFileName), project.ProjectShortName);
            File.WriteAllText(masterSolutionPath, slnText);

            if (project.ProjectIs.HasFlag(CProjectIs.DataBase))
            {
                //fixup the project. turn the SqlProj back into true SqlProj
                var solutionFileText = File.ReadAllText(masterSolutionPath);

                solutionFileText = solutionFileText.Replace(
                    $@"Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{project.ProjectName}""",
                    $@"Project(""{{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}}"") = ""{project.ProjectName}""");
                File.WriteAllText(masterSolutionPath, solutionFileText);


                FixupSqlProjFiles(project, outputRootPath);
            }
        }

        private void FixupSqlProjFiles(CProject project, string outputRootPath)
        {
           
                var fileText = File.ReadAllText(project.Path);
                fileText = fileText.Replace(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild",
                    "$(MSBuildExtensionsPath)");
                File.WriteAllText(project.Path, fileText, Encoding.UTF8);
           
        }


    }

 
}