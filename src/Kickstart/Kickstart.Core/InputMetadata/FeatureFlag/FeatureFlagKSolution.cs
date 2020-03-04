using System.Diagnostics.CodeAnalysis;
using Kickstart.Interface;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;

namespace Kickstart.InputMetadata.FeatureFlag
{

    [ExcludeFromCodeCoverage]
    public class FeatureFlagKSolution : KSolution
    {
        public FeatureFlagKSolution()
        {
            CompanyName = "Company";
            SolutionName = "FeatureFlag";
            ProtoFolder = "FeatureFlag";
            ProtoFileName = "FeatureFlag.proto";
            /*
            DefaultSchemaName = "AvailGateway";
            KickstartdatabaseProject = false;

            KickstartCRUDStoredProcedures = false;

            KickstartTableSeedScripts = false;
            KickstartDataProject = false;
            KickstartGrpcProjectFlag = false;
            KickstartGrpcIntegrationProjectFlag = true;
              */
            /*
            var kProtoFile = new ProtoToKProtoConverter(null).BuildProtoFileFromKSolution(this);

            var databaseProject = new KDatabaseProject { ProjectName = SolutionName };
            databaseProject.InferSqlFromProto(kProtoFile); 
            Project.Add(databaseProject);
            var dataLayerProject = new KDataLayerProject { ProjectName = SolutionName };
            dataLayerProject.DependsOnProject.Add(databaseProject);
            Project.Add(dataLayerProject);

            var grpcProject = new KGrpcProject {ProjectName = SolutionName};
            grpcProject.DependsOnProject.Add(databaseProject);
            grpcProject.DependsOnProject.Add(dataLayerProject);

            grpcProject.ProtoFile.Add(kProtoFile);
            Project.Add(grpcProject);
            Project.Add(new KGrpcServiceClientTestProject {ProjectName = SolutionName});
            Project.Add(new KDockerBuildScriptProject {ProjectName = SolutionName});

            Project.Add(new KGrpcServiceUnitTestProject {ProjectName = SolutionName});
            Project.Add(new KGrpcServiceClientTestProject {ProjectName = SolutionName});
            Project.Add(new KGrpcServiceIntegrationTestDbProject {ProjectName = SolutionName});
            Project.Add(new KGrpcServiceIntegrationTestProject {ProjectName = SolutionName});
            Project.Add(new KGrpcIntegrationBusinessTestProject {ProjectName = SolutionName});
            */
            /*
            MetadataConfigService = GetMetadataConfigService();
            SeedDataService = GetSeedDataService();
            ProtoFileFile = "AvailGateway.proto.json";


            SqlStoredProcedureText = GetStoredProcedureText();
            MockSqlViewText = GetMockSqlViewText();
            SqlTableTypeText = GetSqlTableTypeText();
            */
        }

        
        private string GetSqlTableTypeText()
        {
            return @"";
        }

        private string GetMockSqlViewText()
        {
            return @"";
        }

        private string GetStoredProcedureText()
        {
            return @"";
        }

        public IMetadataConfigService GetMetadataConfigService()
        {
            return null;
        }

        public ISeedDataService GetSeedDataService()
        {
            return null;
        }
    }
}