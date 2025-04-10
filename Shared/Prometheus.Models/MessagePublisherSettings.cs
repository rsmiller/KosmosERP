using Prometheus.Models.Interfaces;

namespace Prometheus.Models;

public class MessagePublisherSettings : IMessagePublisherSettings
{
    public string? account_provider { get; set; }
    public string? rabbitmq_host { get; set; }
    public string? rabbitmq_username { get; set; }
    public string? rabbitmq_password { get; set; }
    public string? rabbitmq_port { get; set; }
    public string? rabbitmq_virtual_host { get; set; }
    public string? rabbitmq_exchange { get; set; }
    public string? rabbitmq_routing_key { get; set; }
    public string? aws_region { get; set; }
    public string? azure_connection_string { get; set; }
    public string? transaction_movement_topic { get; set; }
}
