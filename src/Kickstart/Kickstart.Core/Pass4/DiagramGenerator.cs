using System.Collections.Generic;
using System.IO;
using Kickstart.Pass1.KModel;
using Kickstart.Pass4.Diagram;

namespace Kickstart.Pass4
{
    class DiagramGenerator
    {

        public static void GenerateDiagram(string outputRootPath, List<KSolutionGroup> solutionGroupList)
        {
            var diagramService = new MSolutionListToPlantUmlConverter();
            var solutionDiagram1 = diagramService.Convert(solutionGroupList);

            diagramService.Save(Path.Combine(outputRootPath, "Diagram1WithoutGroups.uml"));

            var solutionDiagram2 = diagramService.Convert(solutionGroupList, true);
            diagramService.Save(Path.Combine(outputRootPath, "Diagram2WithGroups.uml"));
        }

    }
}
