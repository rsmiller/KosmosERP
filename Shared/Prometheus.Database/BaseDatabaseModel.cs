
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Database;
public class BaseDatabaseModel
{
    [Required]
    [Key]
    public int id { get; set; }

    [Required]
    public bool is_deleted { get; set; } = false;

    [Required]
    public DateTime created_on { get; set; }

    [Required]
    public int created_by { get; set; }

    public DateTime? updated_on { get; set; }
    public int? updated_by { get; set; }

    public DateTime? deleted_on { get; set; }

    public int? deleted_by { get; set; }
}

