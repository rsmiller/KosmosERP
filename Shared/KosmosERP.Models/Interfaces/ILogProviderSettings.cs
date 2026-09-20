
namespace KosmosERP.Models.Interfaces;
public interface ILogProviderSettings
{
    string? log_provider { get; set; }
    string? application_insights_connection_string { get; set; }
    string? datadog_api_key { get; set; }
    string? datadog_endpoint { get; set; }
    string? datadog_environment { get; set; }
    string? database_connection_string { get; set; }
}