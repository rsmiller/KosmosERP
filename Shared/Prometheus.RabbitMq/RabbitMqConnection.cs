
namespace Prometheus.RabbitMq;

public interface IRabbitMqConnection
{
    string HostName { get; set; }
    string UserName { get; set; }
    string Password { get; set; }
    string VirtualHost { get; set; }
}

public class RabbitMqConnection : IRabbitMqConnection
{
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
}
