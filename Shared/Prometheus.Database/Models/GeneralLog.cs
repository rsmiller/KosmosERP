using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Prometheus.Database.Models;

public class GeneralLog
{
    [Required]
    public int id { get; set; }

    [Required]
    public string category { get; set; } = "";

    [Required]
    public string message { get; set; } = "";

    [Required]
    public DateTime created_on { get; set; } = DateTime.UtcNow;
}
