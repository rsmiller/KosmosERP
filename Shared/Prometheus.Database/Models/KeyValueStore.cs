using System.ComponentModel.DataAnnotations;

namespace Prometheus.Database.Models;

public class KeyValueStore : BaseDatabaseModel
{
    [Required]
    public string key { get; set; } = "";

    [Required]
    public string value { get; set; } = "";

    public string? module_id { get; set; }
}
