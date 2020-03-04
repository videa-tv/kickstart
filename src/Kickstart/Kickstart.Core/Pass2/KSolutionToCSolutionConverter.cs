using System.Collections.Generic;
using System.Linq;
using Kickstart.Interface;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.DataStore;
using Kickstart.Pass2.DataLayerProject;
using Kickstart.Pass2.Docker;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Pass2.IntegrationTestProject;
using Kickstart.Pass2.SqlProject;
using Kickstart.Pass2.UnitTestProject;

using System;
using Kickstart.Pass2.DataStoreProject;
using Kickstart.Utility;
using Microsoft.Extensions.Logging;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.SqlServer;

namespace Kickstart.Pass2
{
    public class KSolutionToCSolutionConverter : IKSolutionToCSolutionConverter
    {
        private readonly ILogger _logger;
        private readonly IGrpcIntegrationServiceProjectService _grpcIntegrationServiceProjectService;
        private readonly IGrpcServiceProjectService _grpcServiceProjectService;

       
        private readonly IGrpcServiceIntegrationTestBusinessProjectService _grpcServiceIntegrationBusinessTestProjectService;
        private readonly IGrpcServiceIntegrationTestProjectService _grpcServiceIntegrationTestProjectService;
        private readonly IFlywayProjectService _flywayProjectService;
        private readonly IKinesisProjectService _kinesisProjectService;

        private readonly IGrpcServiceClientTestProjectService _grpcServiceClientTestProjectService;
        private readonly IBuildScriptService _buildScriptService;
        private readonly IDataLayerServiceFactory _dataLayerServiceFactory;
        //private readonly ISqlServerProjectService _sqlServerProjectService;
        private readonly IDataStoreServiceFactory _dataStoreServiceFactory;
        private readonly IGrpcServiceUnitTestProjectService _grpcServiceUnitTestProjectService;
        private readonly ISolutionFileService _solutionFileService;
        private readonly IDataStoreTestServiceFactory _dataStoreTestServiceFactory;

        public KSolutionToCSolutionConverter(ILogger<KSolutionToCSolutionConverter> logger, 
            IGrpcIntegrationServiceProjectService grpcIntegrationServiceProjectService, 
            IGrpcServiceProjectService grpcServiceProjectService, 
            IGrpcServiceIntegrationTestBusinessProjectService grpcServiceIntegrationBusinessTestProjectService, 
            IGrpcServiceIntegrationTestProjectService grpcServiceIntegrationTestProjectService,
            IGrpcServiceClientTestProjectService grpcServiceClientTestProjectService,
            IBuildScriptService buildScriptService,
            IDataLayerServiceFactory dataLayerServiceFactory, 
            IDataStoreServiceFactory dataStoreServiceFactory,
            IGrpcServiceUnitTestProjectService grpcServiceUnitTestProjectService, 
            ISolutionFileService solutionFileService, 
            IDataStoreTestServiceFactory dataStoreTestServiceFactory)
        {
            _logger = logger;
            _grpcIntegrationServiceProjectService = grpcIntegrationServiceProjectService;
            _grpcServiceProjectService = grpcServiceProjectService;
            _grpcServiceIntegrationBusinessTestProjectService = grpcServiceIntegrationBusinessTestProjectService;
            _grpcServiceIntegrationTestProjectService = grpcServiceIntegrationTestProjectService;
            _grpcServiceClientTestProjectService = grpcServiceClientTestProjectService;
            _buildScriptService = buildScriptService;
            _dataLayerServiceFactory = dataLayerServiceFactory;
            _dataStoreServiceFactory = dataStoreServiceFactory;
            _grpcServiceUnitTestProjectService = grpcServiceUnitTestProjectService;
            _solutionFileService = solutionFileService;
            _dataStoreTestServiceFactory = dataStoreTestServiceFactory;
        }

        public string ConnectionString { get; set; }
        public void Convert(KSolution kSolution)
        {
            var solution = new CSolution { SolutionName = $"{kSolution.CompanyName}.{kSolution.SolutionName}" };
            var integrationTestSolution = new CSolution
            {
                SolutionName = $"{kSolution.CompanyName}.{kSolution.SolutionName}.IntegrationTests"
            };
            bool generatedSomething = true;
            while (generatedSomething)
            {
                generatedSomething = false;
                foreach (var kProject in kSolution.Project)
                {
                    _logger.LogInformation("Generating projects for KProject: {project}", kProject.ProjectName);
                    bool dependenciesAllGenerated = !kProject.DependsOnProject.Any(p => p.GeneratedProject == null);
                    if (dependenciesAllGenerated)
                    {
                        if (kProject.GeneratedProject != null)
                            continue;
                        if (kProject is KWebUIProject)
                        {
                            var webAppProject = kProject as KWebUIProject;
                            var webAppService =
                                   new WebAppProjectService {  };

                            var webAppCProject = webAppService.BuildProject(webAppProject);

                            kProject.GeneratedProject = webAppCProject;
                            if (webAppProject.Kickstart)
                            {
                                solution.Project.Add(webAppCProject);
                            }
                        }

                        if (kProject is KDockerComposeProject)
                        {
                            var dockerComposeKProject = kProject as KDockerComposeProject;

                            var dockerComposeProjectService = new DockerComposeProjectService();

                            var dockerComposeCProject = dockerComposeProjectService.BuildProject(kSolution, dockerComposeKProject);

                            kProject.GeneratedProject = dockerComposeCProject;
                            if (dockerComposeCProject.Kickstart)
                            {
                                solution.Project.Add(dockerComposeCProject);
                            }

                        }

                        if (kProject is KGrpcClientProject)
                        {
                            var kGrpcClientProject = kProject as KGrpcClientProject;

                            var grpcClientProjectService = new GrpcClientProjectService();

                            var grpcClientCProject = grpcClientProjectService.BuildProject(kSolution, kGrpcClientProject);

                            kProject.GeneratedProject = grpcClientCProject;
                            if (grpcClientCProject.Kickstart)
                            {
                                solution.Project.Add(grpcClientCProject);
                            }

                        }



                        if (kProject is KDataStoreProject)
                        {
                            var dataStoreKProject = kProject as KDataStoreProject;
                            CProject dataStoreProject = BuildDataStoreProject(kSolution, dataStoreKProject);
                            if (dataStoreProject != null)
                            {
                                kProject.GeneratedProject = dataStoreProject;
                                if (dataStoreKProject.Kickstart && dataStoreProject.Kickstart)
                                    solution.Project.Add(dataStoreProject);
                                _logger.LogInformation("Generated data store project {project}", dataStoreProject.ProjectName);
                                generatedSomething = true;

                            }
                            else
                            {
                                kProject.GeneratedProject = new CNullProject();
                                _logger.LogInformation("Kipped data store project {project}", kProject.ProjectName);
                                generatedSomething = true; //tell a white lie
                            }

                        }
                        else if (kProject is KDataLayerProject)
                        {
                            CProject dataProject = null;
                            var sqlKProject = kProject.DependsOnProject.FirstOrDefault(k => k is KDataStoreProject) as KDataStoreProject;
                            var sqlProject = sqlKProject?.GeneratedProject;
                            var allStoredProcedures = new List<CStoredProcedure>();
                            if (sqlProject != null)
                            {
                                foreach (var pc in sqlProject.ProjectContent)
                                    if (pc.Content is CStoredProcedure)
                                        allStoredProcedures.Add(pc.Content as CStoredProcedure);
                            }

                            
                            var dataLayerKProject = kProject as KDataLayerProject;

                            dataProject = _dataLayerServiceFactory.Create(dataLayerKProject.ConnectsToDatabaseType).BuildProject(sqlKProject, dataLayerKProject, allStoredProcedures,
                                sqlKProject?.Table, sqlKProject?.TableType, sqlKProject?.View);
                            kProject.GeneratedProject = dataProject;
                            if (dataLayerKProject.Kickstart)
                            {
                                solution.Project.Add(dataProject);
                            }
                            
                            generatedSomething = true;

                            foreach (var dbTest in kSolution.Project
                                .OfType<KDataStoreTestProject>())
                            {
                                var dataStoreProject = sqlKProject.GeneratedProject;
                                var kDataStoreTestProject = dbTest as KDataStoreTestProject;
                                var service = _dataStoreTestServiceFactory.Create(kDataStoreTestProject.DataStoreType);
                                if (service != null)
                                {
                                    var dbProviderInterface = dataProject.Interface.First(i =>
                                        i.InheritsFrom?.InterfaceName == "IDbProvider");
                                    var dbProviderClass = dataProject.Class.First(c =>
                                        c.Implements.Exists(i => i.InterfaceName == dbProviderInterface.InterfaceName));

                                    if (kDataStoreTestProject.Kickstart && kDataStoreTestProject.Kickstart)
                                        solution.Project.Add(service.BuildProject(kSolution, sqlKProject,
                                            kDataStoreTestProject, dataLayerKProject, dataStoreProject,
                                            dbProviderInterface, dbProviderClass));
                                    // kDataStoreTestProject, dataStoreProject));
                                }
                            }


                        }
                        else if (kProject is KGrpcIntegrationProject)
                        {
                            var sqlKProject = kProject.DependsOnProject.FirstOrDefault(k => k is KDataStoreProject) as KDataStoreProject;
                            
                            var dataLayerKProject = kProject.DependsOnProject.FirstOrDefault(k => k is KDataLayerProject) as KDataLayerProject;

                            var kGrpcProject = kProject.DependsOnProject.FirstOrDefault(k => k is KGrpcProject) as KGrpcProject;

                            if (sqlKProject !=null && dataLayerKProject != null)
                            {
                                var sqlProject = sqlKProject.GeneratedProject;
                                var dataLayerProject = dataLayerKProject.GeneratedProject;

                                var grpcIntegrationProject = BuildGrpcProject(kSolution, sqlKProject, dataLayerKProject, kGrpcProject,
                                    solution, sqlProject, dataLayerProject);
                                kProject.GeneratedProject = grpcIntegrationProject;
                                solution.Project.Add(grpcIntegrationProject);
                            }
                            else
                            {
                                
                                var grpcKIntegrationProject = kProject as KGrpcIntegrationProject;

                                var grpcIntegrationProject = _grpcIntegrationServiceProjectService.BuildProject(kSolution,
                                    grpcKIntegrationProject, grpcKIntegrationProject.ProtoRef);
                                kProject.GeneratedProject = grpcIntegrationProject;
                                solution.Project.Add(grpcIntegrationProject);
                            }

                           
                            generatedSomething = true;
                        }
                        else if (kProject is KGrpcProject)
                        {
                            var sqlKProject = kProject.DependsOnProject.FirstOrDefault(k => k is KDataStoreProject) as KDataStoreProject;
                            var sqlProject = sqlKProject?.GeneratedProject;

                            var dataLayerKProject = kProject.DependsOnProject.FirstOrDefault(k => k is KDataLayerProject) as KDataLayerProject;
                            var dataLayerProject = dataLayerKProject?.GeneratedProject;

                            var kGrpcProject = kProject as KGrpcProject;//.DependsOnProject.First(k => k is KGrpcProject) as KGrpcProject;
                            
                            var grpcProject = BuildGrpcProject(kSolution, sqlKProject, dataLayerKProject, kGrpcProject,
                                solution, sqlProject, dataLayerProject);
                            kProject.GeneratedProject = grpcProject;
                            if (kGrpcProject.Kickstart)
                            {
                                solution.Project.Add(grpcProject);
                            }

                            foreach (var grpcUnitTestProject in kSolution.Project.OfType<KGrpcServiceUnitTestProject>())
                            {
                                
                                solution.Project.Add(_grpcServiceUnitTestProjectService.BuildProject(kGrpcProject,
                                    grpcUnitTestProject, grpcProject));
                            }

                            foreach (var grpcClientTestProject in kSolution.Project
                                .OfType<KGrpcServiceClientTestProject>())
                            {
                                solution.Project.Add(_grpcServiceClientTestProjectService.BuildProject(kSolution, kGrpcProject,
                                    grpcClientTestProject, grpcProject));
                            }
                            foreach (var grpcIntegrationBusinessTestProject in kSolution.Project
                                .OfType<KGrpcIntegrationBusinessTestProject>())
                            {
                                var grpcIntegrationTestBusinessProject =
                                    _grpcServiceIntegrationBusinessTestProjectService.BuildProject(
                                        grpcIntegrationBusinessTestProject, grpcProject, kGrpcProject.ProtoFile, false);
                                integrationTestSolution.Project.Add(grpcIntegrationTestBusinessProject);

                                foreach (var grpcIntegrationTestProject in kSolution.Project
                                    .OfType<KGrpcServiceIntegrationTestProject>())
                                {
                                    
                                    integrationTestSolution.Project.Add(
                                        _grpcServiceIntegrationTestProjectService.BuildProject(kGrpcProject,
                                            grpcIntegrationTestProject, grpcIntegrationTestBusinessProject, false));
                                }
                            }
                            generatedSomething = true;
                        }
                    }
                }
            }

            foreach (var mDockerBuildScriptProject in kSolution.Project
                .OfType<KDockerBuildScriptProject>())
                solution.Project.Add(_buildScriptService.Execute(kSolution.SolutionName, mDockerBuildScriptProject));
             
            solution.Project.Add(_solutionFileService.BuildSolutionFilesProject(kSolution));


            kSolution.GeneratedSolution = solution;
            kSolution.GeneratedTestSolution = integrationTestSolution;

            int x = 1;
        }

        private CProject BuildDataStoreProject(KSolution kSolution, KDataStoreProject dataStoreKProject)
        {
             
            var service = _dataStoreServiceFactory.Create(dataStoreKProject.DataStoreType);
            if (service == null)
                return null;

            return service.BuildProject(kSolution, dataStoreKProject);
            
        }

        public void ConvertOld(KSolution kSolution) 
        {
            var solution = new CSolution {SolutionName = $"{kSolution.CompanyName}.{kSolution.SolutionName}"};
            var integrationTestSolution = new CSolution
            {
                SolutionName = $"{kSolution.CompanyName}.{kSolution.SolutionName}.IntegrationTests"
            };

            foreach (var sqlKProject in kSolution.Project
                .OfType<KDataStoreProject>())
            {
                 
                var sqlProject = _dataStoreServiceFactory.Create(sqlKProject.DataStoreType).BuildProject(kSolution, sqlKProject);

                if (sqlKProject.Kickstart)
                    solution.Project.Add(sqlProject);
                var allStoredProcedures = new List<CStoredProcedure>();
                foreach (var pc in sqlProject.ProjectContent)
                    if (pc.Content is CStoredProcedure)
                        allStoredProcedures.Add(pc.Content as CStoredProcedure);

                foreach (var dataLayerProject in kSolution.Project
                    .OfType<KDataLayerProject>())
                {
                    CProject dataProject = _dataLayerServiceFactory.Create(dataLayerProject.ConnectsToDatabaseType).BuildProject(sqlKProject, dataLayerProject, allStoredProcedures, sqlKProject.Table,
                            sqlKProject.TableType, sqlKProject.View);
                        solution.Project.Add(dataProject);
                    
                    foreach (var grpcServiceIntegrationTestDbProject in kSolution.Project
                        .OfType<KGrpcServiceIntegrationTestDbProject>())
                    {
                        var dataIntegrationService = new GrpcServiceIntegrationTestDbProjectService();
                        var dataIntegrationTestProject =
                            dataIntegrationService.BuildProject(grpcServiceIntegrationTestDbProject);
                        integrationTestSolution.Project.Add(dataIntegrationTestProject);
                    }

                    foreach (var kGrpcProject in kSolution.Project
                        .OfType<KGrpcProject>())
                    {

                        var grpcProject = BuildGrpcProject(kSolution, sqlKProject, dataLayerProject, kGrpcProject,
                            solution, sqlProject, dataProject);
                        solution.Project.Add(grpcProject);

                        foreach (var grpcUnitTestProject in kSolution.Project.OfType<KGrpcServiceUnitTestProject>())
                        {
                            var grpcServiceUnitTestProjectService = new GrpcServiceUnitTestProjectService();
                            solution.Project.Add(grpcServiceUnitTestProjectService.BuildProject(kGrpcProject,
                                grpcUnitTestProject, grpcProject));
                        }

                        foreach (var grpcClientTestProject in kSolution.Project
                            .OfType<KGrpcServiceClientTestProject>())
                        {
                            solution.Project.Add(_grpcServiceClientTestProjectService.BuildProject(kSolution, kGrpcProject,
                                grpcClientTestProject, grpcProject));
                        }
                        foreach (var grpcIntegrationBusinessTestProject in kSolution.Project
                            .OfType<KGrpcIntegrationBusinessTestProject>())
                        {
                           
                            var grpcIntegrationTestBusinessProject =
                                _grpcServiceIntegrationBusinessTestProjectService.BuildProject(
                                    grpcIntegrationBusinessTestProject, grpcProject, kGrpcProject.ProtoFile, false);
                            integrationTestSolution.Project.Add(grpcIntegrationTestBusinessProject);

                            foreach (var grpcIntegrationTestProject in kSolution.Project
                                .OfType<KGrpcServiceIntegrationTestProject>())
                            {
                              
                                integrationTestSolution.Project.Add(
                                    _grpcServiceIntegrationTestProjectService.BuildProject(kGrpcProject,
                                        grpcIntegrationTestProject, grpcIntegrationTestBusinessProject, false));
                            }
                        }
                    }
                }
            }

            foreach (var grpcMIntegrationProject in kSolution.Project
                .OfType<KGrpcIntegrationProject>())
            {
                var grpcIntegrationProject = _grpcIntegrationServiceProjectService.BuildProject(kSolution,
                    grpcMIntegrationProject, grpcMIntegrationProject.ProtoRef);
                solution.Project.Add(grpcIntegrationProject);
                foreach (var grpcUnitTestProject in kSolution.Project
                    .OfType<KGrpcServiceUnitTestProject>())
                {
                    solution.Project.Add(_grpcServiceUnitTestProjectService.BuildProject(grpcMIntegrationProject,
                        grpcUnitTestProject, grpcIntegrationProject));
                }

                foreach (var grpcIntegrationBusinessTestProject in kSolution.Project
                    .OfType<KGrpcIntegrationBusinessTestProject>())
                {
                   
                    var project = _grpcServiceIntegrationBusinessTestProjectService.BuildProject(
                        grpcIntegrationBusinessTestProject, grpcIntegrationProject, grpcMIntegrationProject.ProtoFile,
                        true);
                    project.ProjectIs = CProjectIs.Integration | CProjectIs.Test | CProjectIs.Client;
                    integrationTestSolution.Project.Add(project);
                }
                foreach (var grpcClientTestProject in kSolution.Project
                    .OfType<KGrpcServiceClientTestProject>())
                {
                    solution.Project.Add(_grpcServiceClientTestProjectService.BuildProject(kSolution, grpcMIntegrationProject,
                        grpcClientTestProject, grpcIntegrationProject));
                }
            }

            foreach (var mDockerBuildScriptProject in kSolution.Project
                .OfType<KDockerBuildScriptProject>())
                solution.Project.Add(_buildScriptService.Execute(kSolution.SolutionName, mDockerBuildScriptProject));
            solution.Project.Add(_solutionFileService.BuildSolutionFilesProject(kSolution));

            kSolution.GeneratedSolution = solution;
            kSolution.GeneratedTestSolution = integrationTestSolution;
            //return solution;
        }


        private CProject BuildGrpcProject(KSolution kSolution, KDataStoreProject databaseKProject,
            KDataLayerProject dataLayerKProject, KGrpcProject grpcKProjectIn, CSolution solution, CProject sqlProject,
            CProject dataProject)
        {
            
            var grpcProject = _grpcServiceProjectService.BuildProject(kSolution, databaseKProject, dataLayerKProject,
                grpcKProjectIn, sqlProject, dataProject);

            return grpcProject;
        }
    }
}