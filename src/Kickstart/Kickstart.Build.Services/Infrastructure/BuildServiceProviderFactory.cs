using System;
using Microsoft.Extensions.DependencyInjection;
using Kickstart.Build.Services.Startup;

namespace Kickstart.Build.Services.Infrastructure
{
    public class BuildServiceProviderFactory : IServiceProviderFactory<BuildContainer>, IDisposable
    {
        private BuildContainer _container;

        public BuildContainer CreateBuilder(IServiceCollection services)
        {
            _container = new BuildContainer(services);
            return _container;

        }

        public IServiceProvider CreateServiceProvider(BuildContainer containerBuilder)
        {
            return containerBuilder.GetServiceProvider();

        }

        public void Dispose()
        {
            _container?.Dispose();

        }

    }

}
