using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.App.Tests
{
    class MetaRepoBuilder
    {
        private const string company = "https://tfs.company.com/tfs/Company/Company/_git/";

        private const string companyGit = "https://tfs.company.com/tfs/Company/Company%%20Git/_git/";
        
        public CMetaRepo BuildSampleMetaRepo()
        {
            var metaRepo = new CMetaRepo() { MetaRepoName = "BlahAndBlahBlahMega" };


            metaRepo.AddRepos(GetBlahRepos());
            metaRepo.AddRepos(GetBlahBlahRepos());
            metaRepo.AddRepos(GetCommonRepos());
            return metaRepo;
        }
        private List<CRepo> GetCommonRepos()
        {
            var repos = new List<CRepo>();
            repos.Add(new CRepo { Name = "common-repo", Url = company });

            return repos;
        }

        private List<CRepo> GetBlahRepos( )
        {
            var repos = new List<CRepo>();

            var marketPlaceData = new CRepo {Name = "sample-repo", Url = companyGit};
            var marketPlaceDataSolution1 = new CSolution
            {
                SolutionName = "Company.SampleService.SyncServices",
                SolutionPath = @"Code\SyncServices"
            };

            marketPlaceDataSolution1.Project.Add(new CProject
            {
                ProjectName = "SampleService",
                ProjectShortName = "SampleService",
                Path = "Company.SampleService.SyncServices.csproj",
                ProjectIs = CProjectIs.Grpc
            });

            marketPlaceData.RepoSolution.Add(marketPlaceDataSolution1);
            repos.Add(marketPlaceData);


            repos.Add(new CRepo {Name = "news-svc", Url = companyGit});
            repos.Add(new CRepo {Name = "weather-report-svc", Url = companyGit});
           

            repos.Add(new CRepo {Name = "dockerize"});

            return repos;
        }

        
        public List<CRepo> GetBlahBlahRepos()
        {
            var repos = new List<CRepo>();


            var marketData = new CRepo { Name = "market-data", Url = companyGit };
            repos.Add(marketData);

            repos.Add(new CRepo { Name = "stocks-svc", Url = companyGit });
            repos.Add(new CRepo { Name = "warehouse-svc", Url = companyGit });
        
            repos.Add(new CRepo { Name = "dockerize" });

            return repos;
        }
    }
}
