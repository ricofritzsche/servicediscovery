using System;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServiceDiscovery.Consul
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterForServiceDiscovery (this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHostedService, ConsulService>();
            services.Configure<ConsulConfiguration>(configuration);
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = configuration["ConsulConfiguration:Address"];
                consulConfig.Address = new Uri(address);
            }));

            services.AddSingleton<Func<IConsulClient>>(p => () => new ConsulClient(consulConfig =>
            {
                var address = configuration["ConsulConfiguration:Address"];
                consulConfig.Address = new Uri(address);
            }));

            return services;
        }
    }
}
