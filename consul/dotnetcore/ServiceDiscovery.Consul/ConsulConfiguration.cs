namespace ServiceDiscovery.Consul
{
    public sealed class ConsulConfiguration
    {
        public object ServiceID { get; set; }
        public string ServiceName { get; set; }
        public int Port { get; set; }
        public string[] Tags { get; set; }
    }
}