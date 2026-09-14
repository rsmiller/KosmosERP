using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class PurchaseOrderHeader : BaseDatabaseModel
{
    [Required]
    public int vendor_id { get; set; }

    [Required]
    [MaxLength(2)]
    public string po_type { get; set; }

    [Required]
    public int revision_number { get; set; } = 1;

    [Required]
    public int po_number { get; set; }

    [Precision(14, 3)]
    [Required]
    public decimal price { get; set; }

    [Precision(14, 3)]
    [Required]
    public decimal tax { get; set; } = 0;

    [MaxLength(500)]
    public string? deleted_reason { get; set; }

    [MaxLength(500)]
    public string? canceled_reason { get; set; }

    [Required]
    public bool is_complete { get; set; } = false;

    [Required]
    public bool is_canceled { get; set; } = false;

    public DateTime? completed_on { get; set; }
    public int? completed_by { get; set; }
    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }

    [MaxLength(50)]
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<PurchaseOrderLine> purchase_order_lines { get; set; } = new List<PurchaseOrderLine>();

    [NotMapped]
    public Vendor vendor { get; set; }
}
