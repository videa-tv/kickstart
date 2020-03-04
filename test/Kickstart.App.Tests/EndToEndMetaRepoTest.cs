using System;
using System.IO;
using System.Text;
using Kickstart.Interface;
using Kickstart.Pass1;
using Kickstart.Pass2;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Git;
using Kickstart.Pass2.DockerComposeProject;
using Kickstart.Pass3;
using Kickstart.Pass3.Docker;
using Kickstart.Pass3.Git;
using Kickstart.Pass3.VisualStudio2017;
using Kickstart.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StructureMap;

namespace Kickstart.App.Tests
{
    [TestClass]
    public class EndToEndMetaRepoTest
    {
        const string OutputDir = @"c:\Source\meta7\";

        [TestMethod]
        public void BuildSellersMegaSolution()
        {
            var tempPath = OutputDir;

            var codeWriter = new CodeWriter();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IVisualStudioSolutionWriter, VisualStudioSolutionWriter>();
            serviceCollection.AddTransient<ICSolutionVisitor, FastSolutionVisitor>();

            var fileWriter = new FileWriter();
            fileWriter.RootPath = tempPath;
            serviceCollection.AddSingleton<IFileWriter>(fileWriter);

            serviceCollection.AddSingleton<ILogger<CVisualStudioVisitorBase>>(new Logger<CVisualStudioVisitorBase>(new NullLoggerFactory()));
            serviceCollection.AddSingleton<ILogger<CMetaRepoVisitor>>(new Logger<CMetaRepoVisitor>(new NullLoggerFactory()));
            serviceCollection.AddSingleton<ILogger<CRepoVisitor>>(new Logger<CRepoVisitor>(new NullLoggerFactory()));
            serviceCollection.AddSingleton<ILogger<VisualStudioSolutionWriter>>(new Logger<VisualStudioSolutionWriter>(new NullLoggerFactory()));
            serviceCollection.AddSingleton<ILogger<CProjectVisitor>>(new Logger<CProjectVisitor>(new NullLoggerFactory()));
            serviceCollection.AddSingleton<ILogger<CProjectFileVisitor>>(new Logger<CProjectFileVisitor>(new NullLoggerFactory()));
            serviceCollection.AddSingleton<ILogger<CProjectContentVisitor>>(new Logger<CProjectContentVisitor>(new NullLoggerFactory()));
            serviceCollection.AddSingleton<ILogger<CDockerComposeFileVisitor>>(new Logger<CDockerComposeFileVisitor>(new NullLoggerFactory()));
            serviceCollection.AddSingleton<ILogger<CDockerFileServiceVisitor>>(new Logger<CDockerFileServiceVisitor>(new NullLoggerFactory()));
            
            serviceCollection.AddSingleton<ICodeWriter>(codeWriter);
            serviceCollection.AddTransient<ICVisualStudioVisitorBase, CVisualStudioVisitorBase>();

            serviceCollection.AddTransient<IFastSolutionVisitor, FastSolutionVisitor>();
            //serviceCollection.AddTransient<ICVisualStudioVisitor, CVisualStudioVisitor>();
            serviceCollection.AddTransient<ICMetaRepoVisitor, CMetaRepoVisitor>();
            serviceCollection.AddTransient<ICRepoVisitor, CRepoVisitor>();
            serviceCollection.AddTransient<ICVisualStudioVisitor, CVisualStudioVisitor>();

            //var serviceProvider = serviceCollection.BuildServiceProvider();

            var container = new Container();
            container.Configure(config =>
            {
                config.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<ILogger>();
                   
                    scanner.AssemblyContainingType<IKickstartService>();
                    scanner.SingleImplementationsOfInterface();
                    
                });
                config.Populate(serviceCollection);
            });
            var serviceProvider = container.GetInstance<IServiceProvider>();

            var metaRepo = new MetaRepoBuilder().BuildSampleMetaRepo();
            var visitor = serviceProvider.GetRequiredService<ICVisualStudioVisitorBase>(); // new CVisualStudioVisitor(new Mock<ILogger>().Object, codeWriter);
            metaRepo.Accept(visitor as IVisitor);

            Directory.CreateDirectory(tempPath);

            //http://thesoftwarecondition.com/blog/2014/06/08/nuget-issues-with-nested-solutions-branches/

            //get code from repos
            CommandProcessor.ExecuteCommandWindow(codeWriter.ToString(), tempPath, true);

            UpdateLibraryPaths(tempPath);
            var compositeSolution = new CSolution() { SolutionName = metaRepo.MetaRepoName };
            compositeSolution.SolutionPath = Path.Combine(tempPath,$"{compositeSolution.SolutionName}.sln");
            var compositeRepo = new CRepo();
            
            compositeRepo.RepoSolution.Add(compositeSolution);

            metaRepo.CompositeRepo = compositeRepo;


            //todo: dockerize Asp.Net apps
            //set docker-compose to looking in the Publish folder otherwise there's way too many files 
            //add docker file to project
            //set dockerfile to copy when Publish


            //find all the solutions in repos
            foreach (var repo in metaRepo.Repos)
            {
                var repoPath = Path.Combine(tempPath, repo.Name);
                var solutionPaths = new VisualStudioScanner().ScanForSolutions(repoPath);

                var solutions = new SlnToCSolutionConverter().Convert(solutionPaths);
                repo.RepoSolution.Clear();//remove any existing
                foreach (var solution in solutions)
                {
                    if (repo.SolutionWhiteList.Count > 0 && !repo.SolutionWhiteList.Contains(solution.SolutionName))
                    {
                        continue;
                    }
                    repo.RepoSolution.Add(solution);

                }

            }

            var dockerComposeFile = new DockerComposeFileService().Build(metaRepo);

            var dockerComposeProject = new CProject() { ProjectName = "docker-compose", ProjectIs = CProjectIs.DockerCompose, ProjectType = CProjectType.DockerProj, TemplateProjectPath = @"templates\Docker\docker-compose.dcproj" };

            dockerComposeProject.ProjectContent.Add(new CProjectContent { Content = dockerComposeFile, File = new CFile { FileName = "docker-compose.yml", Encoding = Encoding.ASCII  }, BuildAction = CBuildAction.None });
            compositeSolution.Project.Add(dockerComposeProject);

            
            var writer = serviceProvider.GetRequiredService<IVisualStudioSolutionWriter>();

            writer.Write(tempPath, compositeSolution);


            //add all the projects to the composite solution
            var fastSolutionVisitor = serviceProvider.GetRequiredService<IFastSolutionVisitor>(); // new FastSolutionVisitor(codeWriter, new Mock<IFileWriter>().Object);

            foreach (var repo in metaRepo.Repos)
            {
                foreach (var solution in repo.RepoSolution)
                {
                   fastSolutionVisitor.AddAllProjectsToMasterSln(solution, tempPath, compositeSolution.SolutionName);
                }
            }


            //generate batch script to do "Nuget Restore" 
            //todo: move into library
            var batchWriter = new CodeWriter();
            foreach (var repo in metaRepo.Repos)
            {
                foreach (var solution in repo.RepoSolution)
                {
                    System.Uri uri1 = new Uri(tempPath);

                    System.Uri uri2 = new Uri(Path.GetDirectoryName(solution.SolutionPath));
                    
                    Uri relativeUri = uri1.MakeRelativeUri(uri2);
                    var relativePath = relativeUri.ToString().Replace("%20", " ");

                    batchWriter.WriteLine($@"copy nuget.exe ""{relativePath}/nuget.exe"" /Y" );
                    batchWriter.WriteLine($"cd {relativePath}");
                    batchWriter.WriteLine($@"Nuget Restore ""{solution.SolutionPath}""");
                    batchWriter.WriteLine($@"del nuget.exe");
                    batchWriter.WriteLine("cd /D  %~dp0"); //go back to where batch was run
                }
            }
            File.WriteAllText(Path.Combine(tempPath, "NugetRestore.cmd"), batchWriter.ToString());
        }

        void UpdateLibraryPaths(string rootPath)
        {
            var files = Directory.GetFiles(rootPath, "*.csproj", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                UpdateLibraryPath(file);
            }

        }
        void UpdateLibraryPath(string fileName)
        {
            Encoding encoding;
            var fileText = File.ReadAllText(fileName);
            using (var reader = new StreamReader(fileName, true))
            {
                reader.Peek(); // you need this!
                encoding = reader.CurrentEncoding;
            }
            fileText = fileText.Replace(@">..\packages\", @">$(SolutionDir)packages\");
            fileText = fileText.Replace(@"'..\packages\", @"'$(SolutionDir)packages\");
            fileText = fileText.Replace(@"""..\packages\", @"""$(SolutionDir)packages\");

            fileText = fileText.Replace(@">..\..\packages\", @">$(SolutionDir)packages\");
            fileText = fileText.Replace(@"'..\..\packages\", @"'$(SolutionDir)packages\");
            fileText = fileText.Replace(@"""..\..\packages\", @"""$(SolutionDir)packages\");
            fileText = fileText.Replace(@")..\..\packages\", @")$(SolutionDir)packages\");
            fileText = fileText.Replace(@">..\..\..\packages\", @">$(SolutionDir)packages\");
            fileText = fileText.Replace(@"'..\..\..\packages\", @">$(SolutionDir)packages\");

          

            
            fileText = fileText.Replace(@"<Error Condition=""!Exists('$(SolutionDir)\.nuget\NuGet.targets')"" Text=""$([System.String]::Format('$(ErrorText)', '$(SolutionDir)\.nuget\NuGet.targets'))"" />", @"");

            if (fileText.Contains(@"..\packages"))
            {
                int x = 1;
            }
            File.WriteAllText(fileName, fileText, encoding);
        }
    }
}
