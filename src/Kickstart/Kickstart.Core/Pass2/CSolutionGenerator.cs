using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass1.KModel;

namespace Kickstart.Pass2
{
    public class CSolutionGenerator : ICSolutionGenerator
    {
        private IKSolutionToCSolutionConverter _solutionToCSolutionConverter;
        public CSolutionGenerator(IKSolutionToCSolutionConverter solutionToCSolutionConverter)
        {
            _solutionToCSolutionConverter = solutionToCSolutionConverter;
        }

        public void GenerateCSolutions(string outputRootPath, string connectionString,
            List<KSolutionGroup> solutionGroupList)
        {
            /*
            var generatedSomething = false;
            foreach (var solutionGroup in solutionGroupList)
            foreach (var solution in solutionGroup.Solution)
            {
                var returnVal = CSolutionGenerator.GenerateCSolution(solution);
                if (returnVal)
                    generatedSomething = true;
                if (solution is KApplicationSolution)
                {
                    var appSolution = solution as KApplicationSolution;
                    var childList = new List<KSolutionGroup>();
                    var childSg = new KSolutionGroup();
                    childSg.Solution.AddRange(appSolution.ChildSolution);

                    childList.Add(childSg);
                    GenerateCSolutions(outputRootPath, connectionString, childList);

                }
            }

            if (generatedSomething)
                GenerateCSolutions(outputRootPath, connectionString, solutionGroupList);
                */
            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                {

                    _solutionToCSolutionConverter.Convert(solution);

                    //var returnVal = CSolutionGenerator.GenerateCSolution(solution);
                }
        

        }
        public bool GenerateCSolution(KSolution solution)
        {
            if (solution.GeneratedSolution != null)
            {
                return false;
                ;
            }
            var allGenerated = true;
            foreach (var project in solution.Project
                .OfType<KGrpcIntegrationProject>())
            foreach (var rpcRef in project.ProtoRef)
                if (rpcRef.RefSolution.GeneratedSolution == null)
                {
                    allGenerated = false;
                    break;
                }
            if (!allGenerated)
                return false;


            _solutionToCSolutionConverter.Convert(solution);

            return true;
        }

    }
}
