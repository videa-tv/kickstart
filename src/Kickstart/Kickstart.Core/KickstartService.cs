using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
 
using Kickstart.Pass1;
using Kickstart.Pass1.Excel;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;
using Kickstart.Pass2;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass3;
using Kickstart.Pass3.VisualStudio2017;
using Kickstart.Pass4;
 
using System.Threading.Tasks;
 
using Microsoft.Extensions.Configuration;

namespace Kickstart
{
    public class KickstartService : IKickstartService
    {
        public event EventHandler<KickstartProgressChangedEventArgs> ProgressChanged;
        private readonly IConfigurationRoot _configuration;
        private readonly ICSolutionGenerator _solutionGenerator;
        private readonly ICodeGenerator _codeGenerator;
        private readonly IFastSolutionVisitor _solutionVisitor;
        private readonly IExcelToKSolutionGroupConverter _excelToKSolutionGroupConverter;
        private readonly IKickstartCoreService _kickstartCoreService;
        //private readonly IKSolutionGroupToExcelConverter _kSolutionGroupToExcelConverter;

        public KickstartService(IConfigurationRoot configuration, ICSolutionGenerator solutionGenerator, ICodeGenerator codeGenerator, IFastSolutionVisitor solutionVisitor, IExcelToKSolutionGroupConverter excelToKSolutionGroupConverter, IKickstartCoreService kickstartCoreService)//, IKSolutionGroupToExcelConverter kSolutionGroupToExcelConverter)
        {
            _configuration = configuration;
            _solutionGenerator = solutionGenerator;
            _codeGenerator = codeGenerator;
            _solutionVisitor = solutionVisitor;
            _excelToKSolutionGroupConverter = excelToKSolutionGroupConverter;
            _kickstartCoreService = kickstartCoreService;
            //_kSolutionGroupToExcelConverter = kSolutionGroupToExcelConverter;
        }
        public async Task ExecuteAsync(string destinationDirectory, string solutionName, List<KSolutionGroup> solutionGroupList)
        {
            if (string.IsNullOrWhiteSpace(solutionName))
            {
                throw new ApplicationException("Solution name not specified");
            }
            var sw = Stopwatch.StartNew();
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 10, ProgressMessage = "Generation Started" });
            var connectionString = "Server=localhost;";
            var timeString = DateTime.Now.ToString("yyyyMMddHHmmss");

            KickstartCoreService.SetCompanyNameOnProjects(solutionGroupList);
            KickstartCoreService.ConfigureMetaData(solutionGroupList, destinationDirectory);
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "" });
            //deploy sql code to temp db, convert to K objects
            _kickstartCoreService.BuildSqlMeta(destinationDirectory, connectionString, solutionGroupList);
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "Sql objects created on WorkDb" });


            AddProtoRefs(solutionGroupList);
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "" });

            //pass 2
            //generate C objects from K objects
            _solutionGenerator.GenerateCSolutions(destinationDirectory, connectionString, solutionGroupList);
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "Pass 2: K object transformed to C objects" });

            //save the K/C meta to Excel
            //Excel can be tweaked, and used as Meta source
            //_kSolutionGroupToExcelConverter.OutputPath = Path.Combine(destinationDirectory, $"{timeString}.xlsx");
            //_kSolutionGroupToExcelConverter.Convert(solutionGroupList);
            
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "" });


            //Pass 3
            //generate Code from C objects
            _codeGenerator.GenerateCode(solutionGroupList, destinationDirectory);
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "Pass 3: Code generated" });

            _solutionVisitor.AddProjectsToApplication(solutionGroupList, destinationDirectory);
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "Pass 3: Projects added to solution" });

            if (solutionGroupList.Count > 1 || solutionGroupList.First().Solution.Count > 1)
            {
                //only do if there are multiple solutions
                _solutionVisitor.AddAllProjectsToMasterSln(solutionGroupList, destinationDirectory, solutionName);
            }
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "" });

            BatchFileService.GenerateRestoreScript(solutionGroupList, destinationDirectory);
            BatchFileService.GenerateBuildScript(solutionGroupList, destinationDirectory);
            BatchFileService.GenerateDeployDbScript(solutionGroupList, destinationDirectory);
            BatchFileService.GenerateRunServicesScript(solutionGroupList, destinationDirectory);
            BatchFileService.GenerateRunTestClientScript(solutionGroupList, destinationDirectory);

            //Pass4
            //create diagrams

            DiagramGenerator.GenerateDiagram(destinationDirectory, solutionGroupList);
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 5, ProgressMessage = "Pass 4: Diagrams generated" });
            ProgressChanged(this, new KickstartProgressChangedEventArgs() { ProgressPercentChange = 100, ProgressMessage = $"Completed in {sw.Elapsed.TotalSeconds}.{sw.Elapsed.Milliseconds}s" });

            


            System.Console.WriteLine($"Completed in {sw.Elapsed.ToString()}");
        }

        private void AddProtoRefs(object solutionGroupList)
        {
            throw new NotImplementedException();
        }
        public  List<KSolutionGroup> GetSolutionGroupListFromCode()
        {

            //todo: scan all types in project

            var otherSolutionGroup = new KSolutionGroup
            {
                SolutionGroupName = "Other",
                Solution = new List<KSolution>
                {
                   
                }
            };
            var solutionGroupList = new List<KSolutionGroup> { otherSolutionGroup };
            return solutionGroupList;
        }

        private List<KSolutionGroup> GetSolutionGroupListFromExcel()
        {
            var unfiliteredSolutionGroupList =
                _excelToKSolutionGroupConverter.Convert(@"C:\Temp\Company_3_0_20170823151209\20170823151209.xlsx");

            //load View text from file

            LoadViewTextFromFile(unfiliteredSolutionGroupList);
            //blank out the files so we don't get doubled up metadta
            //BlankFileInfo(solutionGroupList);
            return unfiliteredSolutionGroupList;
        }


        private static void BlankFileInfo(List<KSolutionGroup> solutionGroupList)
        {
            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                {
                    foreach (var project in solution.Project
                        .OfType<KDataStoreProject>())
                    {
                        project.SqlTableTypeFile = null;
                        project.SqlStoredProcedureFile = null;
                        project.MockSqlViewFile = null;


                        project.MockSqlViewText = null;

                        project.SqlTableTypeText = null;
                        project.SqlStoredProcedureText = null;
                    }
                    foreach (var project in solution.Project.OfType<KGrpcProject>())
                        foreach (var protoFile in project.ProtoFile)
                        {
                            protoFile.ProtoFileFile = null;


                            protoFile.ProtoFileText = null;
                        }
                }
        }

        private static void LoadViewTextFromFile(List<KSolutionGroup> solutionGroupList)
        {
            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                    foreach (var project in solution.Project
                        .OfType<KDataStoreProject>())
                    {
                        if (string.IsNullOrEmpty(project.SqlViewFile))
                            continue;
                        project.SqlViewText = ResourceFileReader.ReadResourceFile(solution.SolutionName, project.SqlViewFile);
                    }
        }






        /*
private static void GenerateKickstartForIntegration(string destinationDirectory, string connectionString, List<MSolutionGroup> solutionGroupList)
{
   foreach (var solutionGroup in solutionGroupList)
   {
       foreach (var solution in solutionGroup.Solution)
       {
           if (!solution.KickstartGrpcIntegrationProjectFlag)
           {
               continue;//  second pass, GrpcRefs are set
           }

           var solutionConverter = new KSolutionToCSolutionConverter();
           solutionConverter.Convert(solution);
       }
   }
}*/

        private static void AddProtoRefs(List<KSolutionGroup> solutionGroupList)
        {
            #region
            KickstartCoreService.AddProtoRef(solutionGroupList, "blah", CProtoRpcRefDataDirection.Pull, "blahblah",
               "blahblahblahService", "GetBlah");
        
            #endregion
        }







        private static List<KSolutionGroup> GetFilteredSolutionGroupList(List<KSolutionGroup> solutionGroupList,
            string solutionName)
        {
            var filteredStationGroup = new List<KSolutionGroup>();
            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                    if (solution.SolutionName == solutionName)
                    {
                        filteredStationGroup.Add(new KSolutionGroup { Solution = new List<KSolution> { solution } });
                        return filteredStationGroup;
                    }
            return filteredStationGroup;
        }

        private static List<KSolutionGroup> GetFilteredSolutionGroupList(List<KSolutionGroup> solutionGroupList,
            string[] solutionNames)
        {
            var filteredStationGroup = new List<KSolutionGroup>();
            foreach (var solutionGroup in solutionGroupList)
                foreach (var solution in solutionGroup.Solution)
                    if (solutionNames.Contains(solution.SolutionName))
                        filteredStationGroup.Add(new KSolutionGroup { Solution = new List<KSolution> { solution } });
            return filteredStationGroup;
        }
    }
}
