
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Prometheus.Database.Models;

public class ErrorLog
{
    [Required]
    public int id { get; set; }

    [Required]
    public int error_severity { get; set; }

    [Required]
    [MaxLength(75)]
    public string source { get; set; } = "";

    [Required]
    [MaxLength(200)]
    public string method { get; set; } = "";

    [Required]
    [MaxLength(5000)]
    public string error_message { get; set; } = "";

    [MaxLength(5000)]
    public string inner_message { get; set; } = "";

    [Required]
    public DateTime created_on { get; set; } = DateTime.Now;
}
