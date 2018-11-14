using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceDiscovery.Consul
{
    internal sealed class ConsulService : IHostedService
    {
        private CancellationTokenSource cancellationTokenSource;
        private readonly IConsulClient client;
        private readonly IOptions<ConsulConfiguration> config;
        private readonly ILogger<ConsulService> logger;
        private readonly IServer server;
        private string registrationId;

        public ConsulService(IConsulClient client,
            IOptions<ConsulConfiguration> config,
            ILogger<ConsulService> logger,
            IServer server)
        {
            this.client = client;
            this.config = config;
            this.logger = logger;
            this.server = server;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var features = this.server.Features;
            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.First();

            var uri = new Uri(address);
            this.registrationId = $"{config.Value.ServiceID}-{config.Value.Port}";

            var registration = new AgentServiceRegistration()
            {
                ID = this.registrationId,
                Name = this.config.Value.ServiceName,
                Address = $"{uri.Scheme}://{uri.Host}",
                Port = config.Value.Port,
                Tags = config.Value.Tags,
                Check = new AgentServiceCheck()
                {
                    HTTP = $"{uri.Scheme}://{uri.Host}:{config.Value.Port}/api/health/status",
                    Timeout = TimeSpan.FromSeconds(3),
                    Interval = TimeSpan.FromSeconds(10)
                }
            };

            this.logger.LogInformation($"Register {config.Value.ServiceID} for Consul...");

            await this.client.Agent.ServiceDeregister(registration.ID, this.cancellationTokenSource.Token);
            await this.client.Agent.ServiceRegister(registration, this.cancellationTokenSource.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.cancellationTokenSource.Cancel();
            this.logger.LogInformation($"Deregister {config.Value.ServiceID} from Consul...");
            try
            {
                await this.client.Agent.ServiceDeregister(this.registrationId, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Deregisteration failed");
            }
        }
    }
}
