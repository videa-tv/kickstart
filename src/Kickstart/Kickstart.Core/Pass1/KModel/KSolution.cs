using System.Collections.Generic;
using Kickstart.Pass1.Service;
using Kickstart.Pass2.CModel.Code;

namespace Kickstart.Pass1.KModel
{
    public class KSolution : KPart
    {
        
        public string SolutionName { get; set; }
        /*
        public string DefaultSchemaName  { get; set; }
        
        public string DbSuffix  { get; set; }
        public bool KickstartDatabaseProject { get; set; } = true;
        public bool KickstartStoredProcedures  { get; set; } = true;
        public bool KickstartTableSeedScripts  { get; set; } = true;
        public bool KickstartDataProject  { get; set; } = true;
        public string DataProjectName { get; set; }
        public bool KickstartGrpcProjectFlag  { get; set; } = true;
        public string GrpcProjectName { get; set; }
        public string GrpcProjectShortName { get; set; }
        public bool KickstartGrpcIntegrationProjectFlag  { get; set; } = true;
        */


        //public List<SProtoRpcRef> ProtoRpcRef { get; set; } = new List<SProtoRpcRef>();
        //public bool KickstartAuditTables { get; set; } = true;


        public IList<KProject> Project { get; set; } = new List<KProject>();


        //public List<SProtoRpcRef> ProtoRpcRef2 { get; set; } = new List<SProtoRpcRef>();

        public CSolution GeneratedSolution { get; set; }
        public CSolution GeneratedTestSolution { get; set; }

        public string CompanyName { get; set; } = "Company";
        public string ProtoFolder { get; set; }
        public string ProtoFileName { get; set; }

        public string ProtoFileText => GetProtoFileText(ProtoFolder, ProtoFileName);

        /*
        public bool KickstartGrpcIntegrationBusinessTestProject { get; set; } = true;
        public bool KickstartGrpcIntegrationTestProject { get; set; } = true;
        public bool KickstartGrpcClientTestProject { get; set; } = true;
        public bool KickstartGrpcUnitTestProject { get; set; } = true;
        public bool KickstartDatabaseTestProject { get; set; } = true;
        */
        public void ConfigureMetaData()
        {
            foreach (var project in Project)
                project.ConfigureMetaData();
        }
       
        private string GetProtoFileText(string folderName, string protoFileName)
        {
            return ResourceFileReader.ReadResourceFile(folderName, protoFileName);
        }

    }
}