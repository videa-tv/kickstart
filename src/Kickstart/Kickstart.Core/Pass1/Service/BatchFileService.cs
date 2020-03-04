using System.Collections.Generic;
using System.IO;
using System.Text;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Utility;

namespace Kickstart.Pass1
{
    class BatchFileService
    {

        public static void GenerateRestoreScript(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            var codeWriter = new CodeWriter();
            var fileWriter = new FileWriter(outputRootPath);
            /*
            codeWriter.WriteLine(@"@REM *************************************************************************
            @REM The MSBuild command prompt is used for build environments that handle 
            @REM discovery of the Windows SDK and other tools. In these cases, the
            @REM normal developer command prompt may set environment variables that 
            @REM override the discovery mechanism (e.g. WindowsSdkDir).
            @REM *************************************************************************

            @set VSCMD_BANNER_TEXT_ALT=Visual Studio 2017 MSBuild Command Prompt
            @call ""C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\vsdevcmd.bat"" -no_ext -winsdk=none %*
            @set VSCMD_BANNER_TEXT_ALT =

            :end
            ");*/
            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                {
                    foreach (var project in solution.GeneratedSolution.Project)
                    {
                        if (project.ProjectIs.HasFlag(CProjectIs.DataBase))
                            continue; //todo: only do for .Net Core projects

                        codeWriter.WriteLine(
                            $"dotnet restore {project.Path}   /v:quiet /p:WarningLevel=0 /nowarn:MSB3277 /nowarn:NU1603");
                        //codeWriter.WriteLine($"dotnet msbuild {solution.GeneratedSolution.SolutionPath}");
                        //codeWriter.WriteLine($"msbuild {solution.GeneratedSolution.SolutionPath}");
                    }
                    codeWriter.WriteLine();
                }


            fileWriter.WriteFile("01_RestoreAllSrc.bat", codeWriter.ToString(), Encoding.ASCII);
        }



        public static void GenerateBuildScript(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            var codeWriter = new CodeWriter();
            var fileWriter = new FileWriter(outputRootPath);

            codeWriter.WriteLine(@"@REM *************************************************************************
            @REM The MSBuild command prompt is used for build environments that handle 
            @REM discovery of the Windows SDK and other tools. In these cases, the
            @REM normal developer command prompt may set environment variables that 
            @REM override the discovery mechanism (e.g. WindowsSdkDir).
            @REM *************************************************************************

            @set VSCMD_BANNER_TEXT_ALT=Visual Studio 2017 MSBuild Command Prompt
            @call ""C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\vsdevcmd.bat"" -no_ext -winsdk=none %*
            @set VSCMD_BANNER_TEXT_ALT =

            :end
            ");


            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                {
                    //codeWriter.WriteLine($"dotnet restore {solution.GeneratedSolution.SolutionPath}");
                    //codeWriter.WriteLine($"dotnet msbuild {solution.GeneratedSolution.SolutionPath}");
                    codeWriter.WriteLine(
                        $"msbuild {solution.GeneratedSolution.SolutionPath}   /v:minimal /p:WarningLevel=0 /nowarn:MSB3277 /nowarn:NU1603");
                    codeWriter.WriteLine();
                }

            fileWriter.WriteFile("02_BuildAllSrc.bat", codeWriter.ToString(), Encoding.ASCII);
        }

        public static void GenerateDeployDbScript(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            var codeWriter = new CodeWriter();
            var fileWriter = new FileWriter(outputRootPath);
            codeWriter.WriteLine("set curdir=%cd%");
            codeWriter.WriteLine();
            codeWriter.WriteLine(@"@REM *************************************************************************
            @REM The MSBuild command prompt is used for build environments that handle 
            @REM discovery of the Windows SDK and other tools. In these cases, the
            @REM normal developer command prompt may set environment variables that 
            @REM override the discovery mechanism (e.g. WindowsSdkDir).
            @REM *************************************************************************

            @set VSCMD_BANNER_TEXT_ALT=Visual Studio 2017 MSBuild Command Prompt
            @call ""C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\vsdevcmd.bat"" -no_ext -winsdk=none %*
            @set VSCMD_BANNER_TEXT_ALT =

            :end
            ");
            codeWriter.WriteLine();
            codeWriter.WriteLine("cd %curdir%");

            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                {
                    foreach (var project in solution.GeneratedSolution.Project)
                    {
                        if (!project.ProjectIs.HasFlag(CProjectIs.DataBase))
                            continue;
                        var relativePath =
                            BuildRelativePath(Path.GetFullPath(project.Path), Path.GetFullPath(outputRootPath));
                        codeWriter.WriteLine(
                            $@"MSBUILD "".{
                                    relativePath
                                }"" /t:build ""/p:Platform=AnyCPU"" /t:deploy /p:TargetConnectionString=""Data Source = localhost; Integrated Security = True; "" /p:TargetDatabase=""{
                                    project.ProjectShortName
                                }"" /p:Configuration=Release /p:VisualStudioVersion=15.0   /v:minimal /p:WarningLevel=0 /nowarn:MSB3277 /nowarn:NU1603");
                        //codeWriter.WriteLine($"dotnet msbuild {solution.GeneratedSolution.SolutionPath}");
                        //codeWriter.WriteLine($"msbuild {solution.GeneratedSolution.SolutionPath}");
                    }
                    codeWriter.WriteLine();
                }


            fileWriter.WriteFile("03_DeployAllDb.bat", codeWriter.ToString(), Encoding.ASCII);
        }


        public static void GenerateRunServicesScript(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            var codeWriter = new CodeWriter();
            var fileWriter = new FileWriter(outputRootPath);

            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                {
                    foreach (var project in solution.GeneratedSolution.Project)
                    {
                        if (!project.ProjectIs.HasFlag(CProjectIs.Service))
                            continue; //todo: only do for .Net Core projects
                        var relativePath =
                            BuildRelativePath(Path.GetFullPath(project.Path), Path.GetFullPath(outputRootPath));

                        codeWriter.WriteLine(
                            $"start /b dotnet.exe run --project .{relativePath} --no-build  /v:minimal /p:WarningLevel=0 /nowarn:MSB3277 /nowarn:NU1603");
                        //codeWriter.WriteLine($"dotnet msbuild {solution.GeneratedSolution.SolutionPath}");
                        //codeWriter.WriteLine($"msbuild {solution.GeneratedSolution.SolutionPath}");
                        codeWriter.WriteLine("waitfor SomethingThatIsNeverHappening / t 3");
                    }
                    codeWriter.WriteLine();
                }

            fileWriter.WriteFile("04_RunAllServices.bat", codeWriter.ToString(), Encoding.ASCII);
        }

        public static void GenerateRunTestClientScript(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            var codeWriter = new CodeWriter();
            var fileWriter = new FileWriter(outputRootPath);

            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                {
                    foreach (var project in solution.GeneratedSolution.Project)
                    {
                        if (!project.ProjectIs.HasFlag(CProjectIs.Test) && !project.ProjectIs.HasFlag(CProjectIs.Client))
                            continue; //todo: only do for .Net Core projects
                        var relativePath =
                            BuildRelativePath(Path.GetFullPath(project.Path), Path.GetFullPath(outputRootPath));

                        codeWriter.WriteLine(
                            $"dotnet.exe run --project .{relativePath} --no-build /v:minimal /p:WarningLevel=0 /nowarn:MSB3277 /nowarn:NU1603");
                        //codeWriter.WriteLine($"dotnet msbuild {solution.GeneratedSolution.SolutionPath}");
                        //codeWriter.WriteLine($"msbuild {solution.GeneratedSolution.SolutionPath}");
                    }
                    codeWriter.WriteLine();
                }

            fileWriter.WriteFile("05_RunAllTestClient.bat", codeWriter.ToString(), Encoding.ASCII);
        }

        private static string BuildRelativePath(string absolutePath, string basePath)
        {
            return absolutePath.Substring(basePath.Length);
        }

    }
}
