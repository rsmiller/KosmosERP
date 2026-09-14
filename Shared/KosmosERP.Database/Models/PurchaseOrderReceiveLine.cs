using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class PurchaseOrderReceiveLine : BaseDatabaseModel
{
    [Required]
    public int purchase_order_receive_header_id { get; set; }

    [Required]
    public int purchase_order_line_id { get; set; }

    [Required]
    public int units_ordered { get; set; } = 0;

    [Required]
    public int units_received { get; set; } = 0;

    [Required]
    public bool is_complete { get; set; } = false;

    [Required]
    public bool is_canceled { get; set; } = false;

    [MaxLength(500)]
    public string? canceled_reason { get; set; }

    public DateTime? completed_on { get; set; }
    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }

    [MaxLength(50)]
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public PurchaseOrderLine purchase_order_line { get; set; }
}
