using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using Microsoft.Extensions.Logging;
using Kickstart.Build.Services.Query;
using Kickstart.Build.Services.GrpcCommon.Infrastructure;

namespace Kickstart.Build.Services.Implementation
{
    public class HealthServiceImpl : Health.HealthBase
    {
        private readonly IMediatorExecutor _executor;
        private readonly ILogger<HealthServiceImpl> _logger;

        public HealthServiceImpl(IMediatorExecutor executor, ILogger<HealthServiceImpl> logger)
        {
            _executor = executor;
            _logger = logger;

        }

        public override async Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            var status = HealthCheckResponse.Types.ServingStatus.Serving;

            try
            {
                if (!await _executor.ExecuteAsync(new HealthCheckQuery()))
                {
                    status = HealthCheckResponse.Types.ServingStatus.NotServing;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error running health check: {0}", e);
                status = HealthCheckResponse.Types.ServingStatus.NotServing;
            }

            return new HealthCheckResponse
            {
                Status = status
            };

        }

    }

}
