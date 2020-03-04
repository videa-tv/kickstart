using System;
using System.Collections.Generic;
using System.Threading;

using System.Threading.Tasks;
using Grpc.Core;
using MediatR;

namespace Kickstart.Services.NetCore.GrpcCommon.Infrastructure
{
    public interface IMediatorExecutor
    {
        Task<T> ExecuteAsync<T>(IRequest<T> data);
        Task<T> ExecuteAsync<T>(IRequest<T> data, CancellationToken cancellationToken);
        Task NotifyAsync(INotification notification);
        Task NotifyAsync(INotification notification, CancellationToken cancellationToken);
        Task ExecuteAndWriteStreamAsync<TMediatorResult>(IRequest<IAsyncEnumerable<TMediatorResult>> data, IServerStreamWriter<TMediatorResult> streamWriter, CancellationToken cancellationToken) where TMediatorResult : class;
    }
}
