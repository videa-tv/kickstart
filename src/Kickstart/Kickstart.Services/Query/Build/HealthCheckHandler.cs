using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System;

namespace Kickstart.Build.Services.Query
{
    public class HealthCheckHandler : IRequestHandler<HealthCheckQuery, bool>
    {
        //private readonly IDataHealthCheckService _dataHealthCheckService;

        public HealthCheckHandler()//IDataHealthCheckService dataHealthCheckService)
        {
            //_dataHealthCheckService = dataHealthCheckService;

        }

        public Task<bool> Handle(HealthCheckQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //return _dataHealthCheckService.Check();

        }

    }

}
