using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

// TODO: When shipping the units shipped need to be incremented or tagged
public class ShipmentLine : BaseDatabaseModel
{
    [Required]
    public int shipment_header_id { get; set; }

    [Required]
    public int order_line_id { get; set; }

    [Required]
    public int units_to_ship { get; set; } = 0;

    [Required]
    public int units_shipped { get; set; } = 0;

    [Required]
    public bool is_complete { get; set; } = false;

    [Required]
    public bool is_canceled { get; set; } = false;

    public DateTime? completed_on { get; set; }
    public int? completed_by { get; set; }
    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }

    [MaxLength(500)]
    public string? canceled_reason { get; set; }

    [MaxLength(50)]
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public OrderLine order_line {  get; set; }
}
