using System.Collections.Generic;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass1.KModel.Project;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.DataLayerProject;
using Kickstart.Pass2.DataLayerProject.Table;
using Kickstart.Pass2.GrpcServiceProject.Builder;
using Kickstart.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kickstart.Pass2.GrpcServiceProject
{
    /// <summary>
    ///     Creates a service that calls other services
    /// </summary>
    public class GrpcIntegrationServiceProjectService : GrpcServiceProjectServiceBase, IGrpcIntegrationServiceProjectService
    {
        private readonly ContainerClassBuilder _containerClassBuilder;

        public GrpcIntegrationServiceProjectService(ILoggerFactory loggerFactory, IConfigurationRoot configuration,
            IGrpcPortService grpcPortService, IModelToEntityCClassConverter modelToEntityCClassConverter
            ) : base(configuration, grpcPortService, new CTableToCClassConverter(), new EntityToModelCClassConverter(new Logger<EntityToModelCClassConverter>(loggerFactory)), new ModelToProtoCClassConverter(), new CProjectToDockerFileConverter(), new KDataLayerProjectToKProtoFileConverter(), new ServiceImplClassBuilder(), modelToEntityCClassConverter)
        {
        }
        public CProject BuildProject(KSolution mSolution, KGrpcIntegrationProject grpcKIntegrationProject,
            IList<KProtoRef> protoRpcRefs)
        {
            _kSolution = mSolution;
            _grpcKProject = grpcKIntegrationProject;


            var project = new CProject
            {
                ProjectName = grpcKIntegrationProject.ProjectFullName,
                ProjectShortName = grpcKIntegrationProject.ProjectShortName,
                ProjectFolder = grpcKIntegrationProject.ProjectFolder,
                ProjectType = CProjectType.CsProj,
                ProjectIs = CProjectIs.Grpc | CProjectIs.Integration | CProjectIs.Service,
                TemplateProjectPath = @"templates\NetCore31ConsoleApp.csproj"
            };
            
            //AddLoggerClass(project);
            //AddGitIgnoreFile(project);
            AddProtoRpcRefs(project, protoRpcRefs);
            //AddProtoBatchFile(project);
            if (protoRpcRefs.Count > 0)
                AddProtoRefBatchFile(project);
            AddAppClass(project);
            //AddLogEvents(project);

            if (_grpcKProject is KGrpcIntegrationProject)
            { 
                AddApiHosts(project);
                AddLimitsSettings(project);
            }

            AddServiceSettings(project);
            AddVersionTxt(project);
            var containerClass =  _containerClassBuilder.AddContainerClass(_grpcKProject, DataStoreTypes.Unknown, project, null, null,null,null);

            AddStartupAppServices(project, null, null);
            // var mediatorInterface = AddIMediatorInterface(project);
            //AddMediatorClass(project, mediatorInterface);
            AddDockerFile(project);
            AddNugetRefs(project);
            AddAppSettingsJson(project, grpcKIntegrationProject.ProtoFile.FirstOrDefault()?.GeneratedProtoFile);

            foreach (var protoFile in grpcKIntegrationProject.ProtoFile)
            {
                AddProtoFile(project, protoFile.GeneratedProtoFile);
                //AddProtoFileRefs(project, protoFileRefs);
                AddQuery(project, null, protoFile.GeneratedProtoFile);
                AddHandlers(project, null, null, grpcKIntegrationProject, protoFile.GeneratedProtoFile);

                foreach (var service in protoFile.GeneratedProtoFile.ProtoService)
                {
                    var protoNamespace = protoFile.GeneratedProtoFile.CSharpNamespace; //
                    if (string.IsNullOrEmpty(protoNamespace))
                        protoNamespace =
                            $@"{grpcKIntegrationProject.CompanyName}.{grpcKIntegrationProject.ProjectName}.{
                                    grpcKIntegrationProject.ProjectSuffix
                                }.Proto.Types";
                    

                    var implClass = _serviceImplClassBuilder.BuildServiceImplClass(_grpcKProject, service, protoFile.CSharpNamespace);
                    project.ProjectContent.Add(new CProjectContent
                    {
                        Content = implClass,
                        BuildAction = _grpcKProject.DotNetType == DotNetType.Framework ? CBuildAction.Compile : CBuildAction.DoNotInclude,
                        File = new CFile { Folder = $@"Implementation", FileName = $"{implClass.ClassName}.cs" }
                    });
                }
            }
            AddStartupGrpcClass(project, grpcKIntegrationProject.ProtoFile);

            AddProgramClass(project, containerClass);

            return project;
        }

        private void AddProtoRpcRefs(CProject project, IList<KProtoRef> protoRpcRefs)
        {
            foreach (var protoRpcRef in protoRpcRefs)
            {
                var protoRpc = GetProtoRpc(protoRpcRef);
                if (protoRpc == null)
                    continue;

                //todo: generate a stripped down proto file, for only what is referened
                project.ProjectContent.Add(new CProjectContent
                {
                    Content = protoRpc.ProtoService.ProtoFile,
                    BuildAction = CBuildAction.None,
                    File = new CFile
                    {
                        Folder = $@"Proto\ProtoRef\Proto",
                        FileName = $"{protoRpc.ProtoService.ServiceName}.proto"
                    }
                });


                project.ProjectContent.Add(new CProjectContent
                {
                    Content = protoRpc,
                    BuildAction = CBuildAction.DoNotInclude,
                    File = new CFile {Folder = $@"Proto\ProtoRef\Json", FileName = $"{protoRpc.RpcName}.json"}
                });
            }
        }

        private void AddProtoFileRefs(CProject project, List<CProtoFileRef> protoFileRefs)
        {
            foreach (var protoFileRef in protoFileRefs)
            {
                if (protoFileRef == null)
                    continue;
                project.ProjectContent.Add(new CProjectContent
                {
                    Content = protoFileRef,
                    BuildAction = CBuildAction.None,
                    File = new CFile
                    {
                        Folder = $@"Proto",
                        FileName = $"{protoFileRef.ProtoFile.ProtoService.First().ServiceName}.proto"
                    }
                });
            }
        }

       
    }
}