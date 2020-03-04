using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;
using Kickstart.Pass1.SqlServer;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass4;

namespace Kickstart.Pass1
{
    public interface IKickstartCoreService
    {
        void BuildSqlMeta(string outputRootPath, string connectionString,
            List<KSolutionGroup> solutionGroupList);
    }

    public class KickstartCoreService : IKickstartCoreService
    {
        private readonly IDbToKSolutionConverter _dbToKSolutionConverter;

        public KickstartCoreService(IDbToKSolutionConverter dbToKSolutionConverter)
        {
            _dbToKSolutionConverter = dbToKSolutionConverter;
        }

        public static void AddProtoRef(List<KSolutionGroup> solutionGroupList, string solutionName,
            CProtoRpcRefDataDirection direction, string refSolutionName, string refServiceName, string refRpcName)
        {
            var solution = GetSolution(solutionGroupList, solutionName);
            if (solution == null)
                return;
            var project =
                solution.Project.FirstOrDefault(p =>
                    p is KGrpcIntegrationProject) as KGrpcIntegrationProject; //todo: support multipe
            var refSolution = GetSolution(solutionGroupList, refSolutionName);
            if (refSolution == null)
                return;
            project.AddProtoRef(project.ProjectName, direction, refSolution, refServiceName, refRpcName);
        }
        private static KSolution GetSolution(List<KSolutionGroup> solutionGroupList, string solutionName)
        {
            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
                if (solution.SolutionName == solutionName)
                    return solution;
            return null;
        }
        private static void CompileCode(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            {
                var solutionBuilder = new SolutionBuilder();

                solutionBuilder.Compile(solution.GeneratedSolution.SolutionPath, "");
            }
        }



        public static void SetCompanyNameOnProjects(List<KSolutionGroup> solutionGroupList)
        {
            foreach (var solutionGroup in solutionGroupList)
            {
                foreach (var solution in solutionGroup.Solution)
                {
                    foreach (var project in solution.Project)
                        project.CompanyName = solution.CompanyName;
                    if (solution is KApplicationSolution)
                    {
                        var appSolution = solution as KApplicationSolution;
                        foreach (var childSolution in appSolution.ChildSolution)
                        foreach (var project in childSolution.Project)
                            project.CompanyName = solution.CompanyName;
                    }
                }
            }
        }

        public static void ConfigureMetaData(List<KSolutionGroup> solutionGroupList, string outputRootPath)
        {
            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            {
                foreach (var project in solution.Project)
                    if (project is KGrpcProject)
                        project.ConfigureMetaData();
                if (solution is KApplicationSolution)
                {
                    var appSolution = solution as KApplicationSolution;
                    foreach (var childSolution in appSolution.ChildSolution)
                    foreach (var project in childSolution.Project)
                        if (project is KGrpcProject)
                            project.ConfigureMetaData();
                }
            }
        }
        public void BuildSqlMeta(string outputRootPath, string connectionString,
            List<KSolutionGroup> solutionGroupList)
        {
            foreach (var solutionGroup in solutionGroupList)
            {
                foreach (var solution in solutionGroup.Solution)
                {
                    foreach (var project in solution.Project
                        .OfType<KDataStoreProject>())
                    {
                        _dbToKSolutionConverter.BuildSqlMeta(connectionString, outputRootPath, project);
                    }
                    if (solution is KApplicationSolution)
                    {
                        var appSolution = solution as KApplicationSolution;
                        foreach (var childSolution in appSolution.ChildSolution)
                        {
                            foreach (var project in childSolution.Project
                                .OfType<KDataStoreProject>())
                            {
                                _dbToKSolutionConverter.BuildSqlMeta(connectionString, outputRootPath, project);
                            }
                        }
                    }
                }
            }
        }
    }
}
