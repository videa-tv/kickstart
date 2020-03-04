using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.DataLayerProject;
using Kickstart.Pass2.DataLayerProject.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kickstart.Pass2.GrpcServiceProject
{
    public class GrpcServiceProjectService : GrpcServiceProjectServiceBase, IGrpcServiceProjectService
    {
        public GrpcServiceProjectService(ILoggerFactory loggerFactory, IConfigurationRoot configuration, IGrpcPortService grpcPortService, IModelToEntityCClassConverter modelToEntityCClassConverter) : base(configuration, grpcPortService, new CTableToCClassConverter(), new EntityToModelCClassConverter(new Logger<EntityToModelCClassConverter>(loggerFactory)), new ModelToProtoCClassConverter(), new CProjectToDockerFileConverter(), new KDataLayerProjectToKProtoFileConverter(), new ServiceImplClassBuilder(), modelToEntityCClassConverter) 
        {
        }
        public override CProject BuildProject(KSolution kSolution, KDataStoreProject databaseKProject,
            KDataLayerProject dataLayerKProject, KGrpcProject grpcKProject, CProject sqlProject, CProject dataProject)
        {
            var project = base.BuildProject(kSolution, databaseKProject, dataLayerKProject, grpcKProject, sqlProject,
                dataProject);
            project.ProjectIs = CProjectIs.Grpc | CProjectIs.Service;
            return project;
        }

       
    }
}