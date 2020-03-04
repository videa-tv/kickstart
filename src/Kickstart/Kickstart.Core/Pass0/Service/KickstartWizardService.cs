using Kickstart.Pass1;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.Service;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass3.gRPC;
using Kickstart.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickstart.Pass0.Model;
using Kickstart.Pass1.KModel.Project;

namespace Kickstart.Wizard.Service
{
    public class KickstartWizardService : IKickstartWizardService
    {
        IProtoToKProtoConverter _protoToKProtoConverter;
        ISProtoFileToProtoFileConverter _protoFileToProtoFileConverter;
        IKDataLayerProjectToKProtoFileConverter _dataLayerProjectToKProtoFileConverter;
        IDbToKSolutionConverter _dbToKSolutionConverter;
        IDataLayerServiceFactory _dataLayerServiceFactory;
        public  KickstartWizardService(IProtoToKProtoConverter protoToKProtoConverter, ISProtoFileToProtoFileConverter protoFileToProtoFileConverter, IKDataLayerProjectToKProtoFileConverter dataLayerProjectToKProtoFileConverter, IDbToKSolutionConverter dbToKSolutionConverter, IDataLayerServiceFactory dataLayerServiceFactory )
        {
            _protoToKProtoConverter = protoToKProtoConverter;
            _protoFileToProtoFileConverter = protoFileToProtoFileConverter;
            _dataLayerProjectToKProtoFileConverter = dataLayerProjectToKProtoFileConverter;
            _dbToKSolutionConverter = dbToKSolutionConverter;
            _dataLayerServiceFactory = dataLayerServiceFactory;
        }
        public KSolution BuildSolution(KickstartWizardModel kickstartWizardModel)
        {
            var solution = new KSolution() { CompanyName = kickstartWizardModel.CompanyName, SolutionName = kickstartWizardModel.SolutionName };

            KProtoFile kProtoFile = null;
            if (kickstartWizardModel.MetadataSource == MetadataSource.Grpc)
            {
                kProtoFile = kickstartWizardModel.MetadataSource == MetadataSource.Grpc ? _protoToKProtoConverter.Convert("zzz", kickstartWizardModel.ProtoFileText) : null;

                //this will override whatever user typed on first standard VS dialog
                if (kickstartWizardModel.CurrentStep == KickstartWizardModel.Step.ProtoFile)
                {
                    kickstartWizardModel.SolutionName = InferSolutionName(kProtoFile);
                    kickstartWizardModel.ProjectName = InferProjectName(kProtoFile);
                }
                else
                {

                }

                var databaseProject = BuildDatabaseProject(kickstartWizardModel, kProtoFile);
                solution.Project.Add(databaseProject);

                var databaseTestProject = BuildDatabaseTestProject(kickstartWizardModel, kProtoFile);
                solution.Project.Add(databaseTestProject);
                
                //_databaseSqlView.Bind(databaseProject);

                //if (_currentStep == Step.GenerationStart)
                {
                    var dataLayerProject = BuildDataLayerProject(kickstartWizardModel, databaseProject);
                    var grpcProject = BuildGrpcProject(kickstartWizardModel, databaseProject, dataLayerProject, kProtoFile);
                    solution.Project.Add(grpcProject);

                    solution.Project.Add(dataLayerProject);
                }
            }
            else if (kickstartWizardModel.MetadataSource == MetadataSource.SqlScripts)
            {
                var databaseProjectStart = BuildDatabaseProject(kickstartWizardModel, null);

                var connectionString = "Server=localhost;";
                var outputRootPath = string.Empty;

                var databaseProject = _dbToKSolutionConverter.BuildSqlMeta(connectionString, outputRootPath, databaseProjectStart);
                
                var dataLayerProject = BuildDataLayerProject(kickstartWizardModel, databaseProject);

                solution.Project.Add(databaseProject);
                solution.Project.Add(dataLayerProject);
                
                var grpcProject = BuildGrpcProject(kickstartWizardModel, databaseProject, dataLayerProject, null);
                grpcProject.CompanyName = kickstartWizardModel.CompanyName;
                solution.Project.Add(grpcProject);

                var protoFiles = _dataLayerProjectToKProtoFileConverter.Convert(databaseProject, dataLayerProject, grpcProject);
                foreach (var pf in protoFiles)
                {
                    grpcProject.ProtoFile.Add(pf);
                }
                //_databaseSqlStep.Bind(databaseProject);

                var pf3 = protoFiles.FirstOrDefault();
                if (pf3 != null)
                {
                    pf3.ProtoFileText = _protoFileToProtoFileConverter.Convert(pf3.GeneratedProtoFile);
                   // _protoFileView.Bind(pf3);
                }

            }


            if (kickstartWizardModel.CreateGrpcServiceTestClientProject)
                solution.Project.Add(new KGrpcServiceClientTestProject { ProjectName = kickstartWizardModel.ProjectName });
            if (kickstartWizardModel.CreateDockerBuildProject)
                solution.Project.Add(new KDockerBuildScriptProject { ProjectName = kickstartWizardModel.ProjectName });
            if (kickstartWizardModel.CreateGrpcUnitTestProject)
                solution.Project.Add(new KGrpcServiceUnitTestProject { ProjectName = kickstartWizardModel.ProjectName });
            if (kickstartWizardModel.CreateIntegrationTestProject)
                solution.Project.Add(new KGrpcServiceIntegrationTestProject { ProjectName = kickstartWizardModel.ProjectName });

            if (kickstartWizardModel.CreateWebAppProject)
            {
                solution.Project.Add(new KWebUIProject { ProjectName = kickstartWizardModel.ProjectName });
            }
            
            if (kickstartWizardModel.CreateDockerComposeProject)
            {
                solution.Project.Add(new KDockerComposeProject() { ProjectName = kickstartWizardModel.ProjectName});
            }
            
            if (kickstartWizardModel.CreateGrpcClientProject)
            {
                solution.Project.Add(new KGrpcClientProject() { ProjectName = kickstartWizardModel.ProjectName});
            }
            return solution;

        }



        private KGrpcProject BuildGrpcProject(KickstartWizardModel kickstartWizardModel, KDataStoreProject databaseProject, KDataLayerProject dataLayerProject, KProtoFile kProtoFile)
        {
            var grpcProject = new KGrpcProject() { CompanyName = kickstartWizardModel.CompanyName, ProjectName = kickstartWizardModel.ProjectName, Kickstart = kickstartWizardModel.CreateGrpcServiceProject };

            if (kProtoFile != null)
            {
                grpcProject.ProtoFile.Add(kProtoFile);
            }
            grpcProject.DependsOnProject.Add(databaseProject);
            grpcProject.DependsOnProject.Add(dataLayerProject);
            return grpcProject;
        }

        private KDataLayerProject BuildDataLayerProject(KickstartWizardModel kickstartWizardModel, KDataStoreProject databaseProject)
        {
            var dataLayerProject = new KDataLayerProject(kickstartWizardModel.DatabaseType) { CompanyName = kickstartWizardModel.CompanyName, ProjectName = kickstartWizardModel.ProjectName, KickstartBulkStore = false, Kickstart = kickstartWizardModel.CreateDataLayerProject };

            
            var allStoredProcedures = new List<CStoredProcedure>();
            var dataProject = _dataLayerServiceFactory.Create(databaseProject.DataStoreType).BuildProject(databaseProject, dataLayerProject, allStoredProcedures,
            databaseProject?.Table, databaseProject?.TableType, databaseProject?.View);

            if (databaseProject != null)
            {
                dataLayerProject.DependsOnProject.Add(databaseProject);
            }

            return dataLayerProject;
        }

        public KDataStoreProject BuildDatabaseProject(KickstartWizardModel kickstartWizardModel, KProtoFile kProtoFile)
        {
            KDataStoreProject databaseProject = new KDataStoreProject { CompanyName = kickstartWizardModel.CompanyName, ProjectName = kickstartWizardModel.ProjectName, KickstartCRUDStoredProcedures = false, Kickstart = kickstartWizardModel.CreateDatabaseProject };

            databaseProject.GenerateStoredProcAsEmbeddedQuery = kickstartWizardModel.GenerateStoredProcAsEmbeddedQuery;
            databaseProject.ConvertToSnakeCase = kickstartWizardModel.ConvertToSnakeCase;
            databaseProject.DataStoreType = kickstartWizardModel.DatabaseType;

            
            if (kickstartWizardModel.MetadataSource == MetadataSource.Grpc && !kickstartWizardModel.HasSql())
            {
               
                databaseProject.InferSqlFromProto(kProtoFile);
              

            }
            else if (kickstartWizardModel.MetadataSource == MetadataSource.SqlScripts || kickstartWizardModel.HasSql())
            {
                databaseProject.SqlTableText = kickstartWizardModel.SqlTableText;
                databaseProject.SqlTableTypeText = kickstartWizardModel.SqlTableTypeText;
                databaseProject.SqlStoredProcedureText = kickstartWizardModel.SqlStoredProcText;

            }
            

            return databaseProject;
        }

        public KDataStoreTestProject BuildDatabaseTestProject(KickstartWizardModel kickstartWizardModel, KProtoFile kProtoFile)
        {
            var databaseProject = new KDataStoreTestProject { CompanyName = kickstartWizardModel.CompanyName,  ProjectName = kickstartWizardModel.ProjectName,
                Kickstart = kickstartWizardModel.CreateDatabaseTestProject };

            databaseProject.DataStoreType = kickstartWizardModel.DatabaseType;

            return databaseProject;
        }

        private string InferSolutionName(KickstartWizardModel kickstartWizardModel)
        {
            if (kickstartWizardModel.MetadataSource == MetadataSource.Grpc)
            {
                var grpcProject = kickstartWizardModel.SelectedTemplateSolutions.First().Project.FirstOrDefault(p => p is KGrpcProject) as KGrpcProject;
                return InferSolutionName(grpcProject.ProtoFile.FirstOrDefault());
            }
            else if (kickstartWizardModel.MetadataSource == MetadataSource.SqlScripts)
            {
                var databaseProject = kickstartWizardModel.SelectedTemplateSolutions.First().Project.FirstOrDefault(p => p is KDataStoreProject) as KDataStoreProject;
                return databaseProject.InferProjectName();
            }
            else
                throw new NotImplementedException();
        }
        private string InferSolutionName(KProtoFile kProtoFile)
        {
            return kProtoFile.GeneratedProtoFile.ProtoService.First().ServiceName.Replace("Service", "");

        }
        private string InferProjectName(KProtoFile kProtoFile)
        {
            return kProtoFile.GeneratedProtoFile.ProtoService.First().ServiceName.Replace("Service", "");

        }
    }
}
