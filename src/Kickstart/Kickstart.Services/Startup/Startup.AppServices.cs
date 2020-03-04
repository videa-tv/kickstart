using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Kickstart.Services.Config;
using Kickstart.Services.Infrastructure;
using Microsoft.Extensions.Options;

namespace Kickstart.Services.Startup
{
    /*
    public static class ServiceCollectionAppServicesExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection serviceCollection, IConfigurationRoot configuration)
        {
            serviceCollection.AddSingleton<ServiceSettings>(
                 p => p.GetRequiredService<IOptions<ServiceSettings>>().Value);
            
            
            return serviceCollection;

        }
    }*/
}
