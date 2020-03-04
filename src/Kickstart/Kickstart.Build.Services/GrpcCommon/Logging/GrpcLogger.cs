using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kickstart.Build.Services.GrpcCommon.Logging
{
    public class GrpcLogger : Grpc.Core.Logging.ILogger
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public GrpcLogger(ILoggerFactory loggerFactory) : this(loggerFactory, typeof(GrpcLogger))
        {
        }

        private GrpcLogger(ILoggerFactory loggerFactory, Type type)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger(type);
        }

        public Grpc.Core.Logging.ILogger ForType<T>()
        {
            return typeof(T) == GetType() ? this : new GrpcLogger(_loggerFactory, typeof(T));
        }

        public void Debug(string message)
        {
            _logger.LogDebug(message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            _logger.LogDebug(format, formatArgs);
        }

        public void Info(string message)
        {
            _logger.LogInformation(message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            _logger.LogInformation(format, formatArgs);
        }

        public void Warning(string message)
        {
            _logger.LogWarning(message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            _logger.LogWarning(format, formatArgs);
        }

        public void Warning(Exception exception, string message)
        {
            _logger.LogWarning(new EventId(0), exception, message);
        }

        public void Error(string message)
        {
            _logger.LogError(message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            _logger.LogError(format, formatArgs);
        }

        public void Error(Exception exception, string message)
        {
            _logger.LogError(new EventId(0), exception, message);
        }
    }
}
