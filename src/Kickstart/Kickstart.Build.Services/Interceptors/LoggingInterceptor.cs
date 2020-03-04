using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Kickstart.Build.Services.Infrastructure;
using System.Threading;
using System.Collections.Generic;

namespace Kickstart.Build.Services.Interceptors
{
    public class LoggingInterceptor : Interceptor
    {
        private readonly ILogger _logger;
        private readonly LogLevel _logLevel;
        private readonly LoggingInterceptorOptions _options;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger, LogLevel logLevel)
            : this(logger, logLevel, new LoggingInterceptorOptions())
        {
        }

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger, LogLevel logLevel, LoggingInterceptorOptions options)
        {
            _logger = logger;
            _logLevel = logLevel;
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            return LogExecution(context, () => continuation(request, context));
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return LogExecution(context, () => continuation(WrapAsyncStreamReader(requestStream), context));
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return LogExecution(context, () => continuation(request, responseStream, context));
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return LogExecution(context, () => continuation(WrapAsyncStreamReader(requestStream), responseStream, context));
        }

        private WrappedAsyncStreamReader<TRequest> WrapAsyncStreamReader<TRequest>(IAsyncStreamReader<TRequest> requestStream)
        {
            var iteration = 0;
            return new WrappedAsyncStreamReader<TRequest>(requestStream,
                (hasNext, request) => LogStreamReaderIteration(hasNext, request, ref iteration));
        }

        private async Task<TResult> LogExecution<TResult>(ServerCallContext context, Func<Task<TResult>> body)
        {
            var ctx = new Dictionary<string, object>
            {
                {"requestId", Guid.NewGuid().ToString("N")}
            };

            using (_logger.BeginScope(ctx))
            {
                try
                {
                    Log("Start calling {0}", context.Method);

                    var result = await body().ConfigureAwait(false);

                    Log("End calling {0}", context.Method);

                    return result;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error calling {0}", context.Method);
                    throw;
                }
            }
        }

        private Task LogExecution(ServerCallContext context, Func<Task> body)
        {
            return LogExecution(context, async () =>
            {
                await body().ConfigureAwait(false);
                return default(object);
            });
        }

        private void LogStreamReaderIteration<TRequest>(bool hasNext, TRequest requestItem, ref int iteration)
        {
            if (!hasNext)
            {
                Log("End reading stream. Total items: {0}.", iteration);
                return;
            }

            iteration++;
            if (!_options.LogAsyncStreamItems)
            {
                Log("Start reading {0} item from stream.", iteration);
            }
            else
            {
                Log("Start reading {0} item from stream. Request item: {1}", iteration, requestItem);
            }
        }

        private void Log(string message, params object[] args) => _logger.Log(_logLevel, message, args);

        private class WrappedAsyncStreamReader<TRequest> : IAsyncStreamReader<TRequest>
        {
            private readonly IAsyncStreamReader<TRequest> _reader;
            private readonly Action<bool, TRequest> _onMoveNext;

            public WrappedAsyncStreamReader(IAsyncStreamReader<TRequest> reader, Action<bool, TRequest> onMoveNext)
            {
                _reader = reader;
                _onMoveNext = onMoveNext;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                var hasNext = await _reader.MoveNext(cancellationToken).ConfigureAwait(false);
                _onMoveNext(hasNext, hasNext ? _reader.Current : default);
                return hasNext;
            }

            public TRequest Current => _reader.Current;
        }
    }
}
