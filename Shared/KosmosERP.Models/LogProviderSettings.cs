using KosmosERP.Models.Interfaces;

namespace KosmosERP.Models;

public class LogProviderSettings : ILogProviderSettings
{
    public string? log_provider { get; set; }
    public string? application_insights_connection_string { get; set; }
    public string? datadog_api_key { get; set; }
    public string? datadog_endpoint { get; set; }
    public string? datadog_environment { get; set; }
    public string? database_connection_string { get; set; }
}
