using System;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Kickstart.Build.Services.GrpcCommon.Extensions;

namespace Kickstart.Build.Services.GrpcCommon
{
    public class GrpcServerBuilder
    {
        private readonly List<ServerServiceDefinition> _services;
        private ServerPort _insecurePort;
        private readonly GrpcServerOptions _serverOptions;
        private readonly Dictionary<string, ChannelOption> _optionsOverride;
        private readonly List<Interceptor> _interceptors;

        public GrpcServerBuilder()
        {
            _services = new List<ServerServiceDefinition>();
            _serverOptions = new GrpcServerOptions();
            _optionsOverride = new Dictionary<string, ChannelOption>();
            _interceptors = new List<Interceptor>();
        }

        public GrpcServerBuilder AddServices(IEnumerable<ServerServiceDefinition> services)
        {
            foreach (var service in services)
            {
                AddService(service);
            }

            return this;
        }

        public GrpcServerBuilder AddService(ServerServiceDefinition service)
        {
            _services.Add(service);
            return this;
        }

        public GrpcServerBuilder AddInsecurePort(int port, string host = null)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                host = "0.0.0.0";
            }

            _insecurePort = new ServerPort(host, port, ServerCredentials.Insecure);
            return this;
        }

        /// <summary>
        /// Change GrpcServerOptions. Please note it's still possible to override them by using AddGrpcOptions
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public GrpcServerBuilder ConfigureServerOptions(Action<GrpcServerOptions> configure)
        {
            configure?.Invoke(_serverOptions);
            return this;
        }

        /// <summary>
        /// Override or add new ServerOptions with native ChannelOption
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public GrpcServerBuilder AddGrpcOption(ChannelOption option)
        {
            _optionsOverride.SetOption(option);
            return this;
        }

        /// <summary>
        /// Override or add new ServerOptions with collection of native ChannelOption
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public GrpcServerBuilder AddGrpcOptions(IEnumerable<ChannelOption> options)
        {
            foreach (var option in options)
            {
                _optionsOverride.SetOption(option);
            }

            return this;
        }

        /// <summary>
        /// Add one or many interceptors. Actual interceptor assignment will happen during Build() method call
        /// </summary>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public GrpcServerBuilder AddInterceptors(params Interceptor[] interceptors)
        {
            interceptors = interceptors ?? throw new ArgumentNullException(nameof(interceptors));

            _interceptors.AddRange(interceptors);

            return this;
        }

        /// <summary>
        /// Build Server object. Please make sure server.Start() is called
        /// </summary>
        /// <returns></returns>
        public Server Build()
        {
            var server = new Server(GetOptions());
            var interceptors = _interceptors.ToArray();
            foreach (var service in _services)
            {
                var newService = service;

                if (interceptors.Length > 0)
                {
                    newService = service.Intercept(interceptors);
                }

                server.Services.Add(newService);
            }

            _insecurePort = _insecurePort ?? throw new Exception("No grpc server port is configured");
            server.Ports.Add(_insecurePort);

            return server;
        }

        internal IEnumerable<ChannelOption> GetOptions()
        {
            var options = new Dictionary<string, ChannelOption>();
            // accept unlimited number of pings and not send GOAWAY
            options.SetOption(new ChannelOption("grpc.http2.max_ping_strikes", 0));

            if (_serverOptions.MaxConnectionIdleMs > 0)
            {
                options.SetOption(new ChannelOption("grpc.max_connection_idle_ms", _serverOptions.MaxConnectionIdleMs));
            }

            if (_serverOptions.MaxConnectionAgeMs > 0)
            {
                options.SetOption(new ChannelOption("grpc.max_connection_age_ms", _serverOptions.MaxConnectionAgeMs));
            }

            foreach (var option in _optionsOverride)
            {
                options.SetOption(option.Value);
            }

            return options.Values;
        }
    }
}
