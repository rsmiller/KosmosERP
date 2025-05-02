using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class PurchaseOrderReceiveHeader : BaseDatabaseModel
{
    [Required]
    public int purchase_order_id { get; set; }

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
    public PurchaseOrderHeader purchase_order { get; set; }

    [NotMapped]
    public List<PurchaseOrderReceiveLine> purchase_order_receive_lines { get; set; } = new List<PurchaseOrderReceiveLine>();

    [NotMapped]
    public List<PurchaseOrderReceiveUpload> purchase_order_receive_uploads { get; set; } = new List<PurchaseOrderReceiveUpload>();
}
