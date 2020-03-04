using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Kickstart.Build.Services.GrpcCommon.Infrastructure
{
    /// <summary>
    /// A simple wrapper around Mediatr to enable Dependency Injection into Handlers.
    /// </summary>
    public class MediatorExecutor : IMediatorExecutor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MediatorExecutor(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<T> ExecuteAsync<T>(IRequest<T> data)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await svc.Send(data).ConfigureAwait(false);
        }

        public async Task<T> ExecuteAsync<T>(IRequest<T> data, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await svc.Send(data, cancellationToken).ConfigureAwait(false);
        }

        public async Task NotifyAsync(INotification notification)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IMediator>();
            await svc.Publish(notification).ConfigureAwait(false);
        }

        public async Task NotifyAsync(INotification notification, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IMediator>();
            await svc.Publish(notification, cancellationToken).ConfigureAwait(false);
        }

        public async Task ExecuteAndWriteStreamAsync<TMediatorResult>(IRequest<IAsyncEnumerable<TMediatorResult>> data,
            IServerStreamWriter<TMediatorResult> streamWriter, CancellationToken cancellationToken)
            where TMediatorResult : class
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IMediator>();
            var enumerable = await svc.Send(data, cancellationToken).ConfigureAwait(false);

            //#if NETSTANDARD2_1
            await foreach (var item in enumerable.WithCancellation(cancellationToken))
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await streamWriter.WriteAsync(item).ConfigureAwait(false);
                }
            }
            /*#else
                        using var enumerator = enumerable.GetEnumerator();
                        if (await enumerator.MoveNext(cancellationToken).ConfigureAwait(false))
                        {
                            Task<bool> moveNextTask;
                            do
                            {
                                var current = enumerator.Current;
                                moveNextTask = enumerator.MoveNext(cancellationToken);
                                await streamWriter.WriteAsync(current);
                            }
                            while (await moveNextTask.ConfigureAwait(false) && !cancellationToken.IsCancellationRequested);
                        }
            #endif*/
        }
    }
}
