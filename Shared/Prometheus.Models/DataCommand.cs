using System.ComponentModel.DataAnnotations;

namespace Prometheus.Models;

public class DataCommand
{
    [Required]
    public int calling_user_id { get; set; }
}
