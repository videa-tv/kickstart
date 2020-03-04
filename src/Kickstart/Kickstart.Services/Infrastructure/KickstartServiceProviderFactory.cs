using System;
using Microsoft.Extensions.DependencyInjection;

namespace Kickstart.Services
{
    internal class KickstartServiceProviderFactory : IDisposable, IServiceProviderFactory<KickstartContainer>
    {
        private KickstartContainer _container;
        public void Dispose()
        {
            _container?.Dispose();
        }

        public KickstartContainer CreateBuilder(IServiceCollection services)
        {

            _container = new KickstartContainer(services);
            return _container;
        }

        public IServiceProvider CreateServiceProvider(KickstartContainer containerBuilder)
        {
            return containerBuilder.GetServiceProvider();
        }
    }
}