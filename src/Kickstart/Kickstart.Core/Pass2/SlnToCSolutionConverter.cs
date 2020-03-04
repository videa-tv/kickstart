using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Onion.SolutionParser.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2
{
    public class SlnToCSolutionConverter
    {
        public bool IgnoreTestProjects { get; set; } = true;
        public IList<CSolution> Convert(IEnumerable<string> slnPaths)
        {
            List<CSolution> solutions = new List<CSolution>();

            foreach(var slnPath in slnPaths)
            {
                solutions.Add(Convert(slnPath));
            }

            return solutions;
        }
        public CSolution Convert(string slnPath)
        {
            var solution = new CSolution();
            solution.SolutionName = Path.GetFileName(slnPath);
            var rawSolution = SolutionParser.Parse(slnPath);
            solution.SolutionPath = slnPath;
            foreach (var rawProject in rawSolution.Projects)
            {
                //skip "folders"
                if (rawProject.TypeGuid == Guid.Parse("{2150e333-8fdc-42a3-9474-1a3956d46de8}"))
                    continue;
                var fullPath = Path.Combine(Path.GetDirectoryName(slnPath), rawProject.Path);
                if (!File.Exists(fullPath))
                {
                    continue;
                }
                var projectText = File.ReadAllText(fullPath);
                if (IgnoreTestProjects && projectText.Contains("{3AC096D0-A1C2-E12C-1390-A8335801FDAB}"))
                {
                    //test project
                    continue;
                }
                if (IgnoreTestProjects && rawProject.Name.Contains("Test"))
                {
                    //test project
                    continue;
                }

                var project = new CProject { };
                project.ProjectGuid = rawProject.Guid;
                project.ProjectShortName = rawProject.Name;
                project.ProjectName = rawProject.Name;
                project.Path =Path.Combine(Path.GetDirectoryName( slnPath),  rawProject.Path);
                project.HasDockerFile = HasDockerFile(project.Path);
                var projectExtension = Path.GetExtension(project.Path).ToLower();
                if (projectExtension == ".sqlproj")
                {
                    project.ProjectType = CProjectType.SqlProj;
                    project.ProjectIs = CProjectIs.DataBase;
                }

                solution.Project.Add(project);
            }
            return solution;
        }
        private bool HasDockerFile(string path)
        {
            if (!File.Exists(path))
                return false;
            var projectText = File.ReadAllText(path);
            if (projectText.Contains(@"Dockerfile"))
            {
                return true;
            }
            return false;
        }
        /*
        private CProjectIs InferProjectIs(string path)
        {
            var projectText = File.ReadAllText(path);
            if (projectText.Contains(" < OutputType>Exe</OutputType>"))
            {
                //console app, lets check for a .proto
                if (projectText.Contains(""))
                {
                    return CProjectIs.Grpc;
                }
            }

        }*/
    }
    }
