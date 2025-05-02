
namespace KosmosERP.Models.Interfaces;

public interface IMessagePublisherSettings
{
    string? account_provider { get; set; }
    string? rabbitmq_host { get; set; }
    string? rabbitmq_username { get; set; }
    string? rabbitmq_password { get; set; }
    string? rabbitmq_port { get; set; }
    string? rabbitmq_virtual_host { get; set; }
    string? rabbitmq_exchange { get; set; }
    string? rabbitmq_routing_key { get; set; }
    string? aws_region { get; set; }
    string? azure_connection_string { get; set; }
    string? transaction_movement_topic { get; set; }
}
