using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Company.KickstartBuild.Services.Types;
using Kickstart.Build.Services.Query;
using Kickstart.Build.Services.Config;
using Kickstart.Build.Services.Auth;
using System;
using Kickstart.Build.Services.GrpcCommon.Infrastructure;

namespace Kickstart.Build.Services
{
    public class KickstartBuildServiceImpl : KickstartBuildService.KickstartBuildServiceBase
    {
        private readonly IMediatorExecutor _executor;
        private readonly AuthenticationSettings _authSettings;

        public KickstartBuildServiceImpl(IMediatorExecutor executor, AuthenticationSettings authSettings)
        {
            _executor = executor;
            _authSettings = authSettings;

        }

        public override async Task<CreateReleaseDefinitionResponse> CreateReleaseDefinition(
            CreateReleaseDefinitionRequest request, 
            ServerCallContext context)
        {
            context.CheckAuthenticated(_authSettings);

            if (!request.ReleaseDefinition.Environments.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Environments were not defined"));
            }

            if (request.ReuseAwsStackFromServiceName == null)
            {
                request.ReuseAwsStackFromServiceName = request.ServiceName;
            }

            if (request.ConfigurationFolder == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ConfigurationFolder was not defined"));
            }

            if (request.ServiceFolder == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ServiceFolder was not defined"));
            }

            var result = await _executor.ExecuteAsync(new CreateReleaseDefinitionQuery
            {
                ServiceName = request.ServiceName,
                ReuseAwsStackFromServiceName = request.ReuseAwsStackFromServiceName,
                ServiceFolder = request.ServiceFolder,
                ConfigurationFolder = request.ConfigurationFolder,
                IsProdPath = request.IsProdPath,
                NeedsAwsUserAccountCreated = request.NeedsAwsUserAccountCreated,
                ReleaseDefinition = request.ToModel(),
                DatabaseServers = request.DatabaseServers.ToModel()
            }).ConfigureAwait(false);


            var response = new CreateReleaseDefinitionResponse();

            if (result != null)
            {
                response.ReleaseDefinition = result.ToProto();

            }
            return response;


        }

        public override async Task<CreateReleaseResponse> CreateRelease(
            CreateReleaseRequest request, 
            ServerCallContext context)
        {
            context.CheckAuthenticated(_authSettings);

            var result = await _executor.ExecuteAsync(new CreateReleaseQuery
            {
                ReleaseDefinition = request.ReleaseDefinition.ToModel()

            }).ConfigureAwait(false);


            var response = new CreateReleaseResponse
            {
            };

            return response;


        }

        public override async Task<DeployReleaseResponse> DeployRelease(
            DeployReleaseRequest request, 
            ServerCallContext context)
        {
            context.CheckAuthenticated(_authSettings);

            var result = await _executor.ExecuteAsync(new DeployReleaseQuery
            {
                ReleaseIdentifier = request.ReleaseIdentifier,
                EnvironmentIdentifier = request.EnvironmentIdentifier,
                ProjectId = Guid.Parse( request.ProjectId)
                
            }).ConfigureAwait(false);


            var response = new DeployReleaseResponse
            {
            };

            return response;


        }

        public override async Task<QueueBuildResponse> QueueBuild(QueueBuildRequest request, ServerCallContext context)
        {
            context.CheckAuthenticated(_authSettings);

            var result = await _executor.ExecuteAsync(new QueueBuildQuery
            {
                BuildDefinition = request.BuildDefinition.ToModel()

            }).ConfigureAwait(false);


            var response = new QueueBuildResponse();

            if (result != null)
            {
                response.Build = result.ToProto();

            }
            return response;


        }

        public override async Task<CreateBuildDefinitionResponse> CreateBuildDefinition(
            CreateBuildDefinitionRequest request, 
            ServerCallContext context)
        {
            context.CheckAuthenticated(_authSettings);
            
            var result = await _executor.ExecuteAsync(new CreateBuildDefinitionQuery
            {
                ServiceName = request.ServiceName,
                BuildDefinition = request.BuildDefinition.ToModel()

            }).ConfigureAwait(false);


            var response = new CreateBuildDefinitionResponse();

            if (result != null)
            {
                response.BuildDefinition = result.ToProto();

            }
            return response;


        }

    }

}
