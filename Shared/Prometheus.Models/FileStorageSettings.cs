using Prometheus.Models.Interfaces;

namespace Prometheus.Models;

public class FileStorageSettings : IFileStorageSettings
{
    public string? account_provider { get; set; }
    public string? azure_connection_string { get; set; }
    public string? azure_container_name { get; set; }
    public string? azure_access_key { get; set; }
    public string? aws_access_key { get; set; }
    public string? aws_secret_key { get; set; }
    public string? aws_bucket_name { get; set; }
    public string? aws_region { get; set; }
    public string? gpc_json_file_path { get; set; }
    public string? gpc_bucket_name { get; set; }
}
