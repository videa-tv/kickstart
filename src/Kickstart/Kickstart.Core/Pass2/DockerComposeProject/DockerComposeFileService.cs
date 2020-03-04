using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Docker;
using Kickstart.Pass2.CModel.Git;
using Kickstart.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Pass2.DockerComposeProject
{
    public class DockerComposeFileService
    {
        public CDockerComposeFile Build(CMetaRepo metaRepo)
        {
            //todo: switch to Visitor pattern
            var relativePathBuilder = new RelativePathBuilder();
            var dockerComposeFile = new CDockerComposeFile();
            foreach (var repo in metaRepo.Repos)
            {
                foreach (var solution in repo.RepoSolution)
                {
                    foreach (var project in solution.Project)
                    {
                        if (!project.HasDockerFile)
                        {
                            continue;
                        }

                        //todo: move this into domain model ?
                        var relativePath = relativePathBuilder
                            .GetRelativePath(metaRepo.CompositeRepo.RepoSolution.First().SolutionPath,
                                Path.GetDirectoryName(project.Path)).Replace(@"\", @"/");

                        dockerComposeFile.Service.Add(new CDockerFileService
                        {
                            ServiceName = project.ProjectShortName.ToLower().Replace(".", "_"),
                            Context = $"./{relativePath}"
                        });
                    }
                }
            }

            return dockerComposeFile;
        }

        public CDockerComposeFile Build()
        {
            var dockerComposeFile = new CDockerComposeFile();

            dockerComposeFile.Service.Add(new CDockerFileService
            {
              //  ServiceName = project.ProjectShortName.ToLower().Replace(".", "_"),
              //  Context = $"./{relativePath}"
            });

            return dockerComposeFile;
        }
    }
}
