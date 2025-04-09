
namespace Prometheus.Models.Interfaces;

public interface IFileStorageSettings
{
    string? account_provider { get; set; }
    string? azure_connection_string { get; set; }
    string? azure_container_name { get; set; }
    string? azure_access_key { get; set; }
    string? aws_access_key { get; set; }
    string? aws_secret_key { get; set; }
    string? aws_bucket_name { get; set; }
    string? aws_region { get; set; }
    string? gpc_json_file_path { get; set; }
    string? gpc_bucket_name { get; set; }
}
