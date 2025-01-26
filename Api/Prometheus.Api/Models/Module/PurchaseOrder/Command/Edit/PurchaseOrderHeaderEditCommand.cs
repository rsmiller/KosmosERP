using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.PurchaseOrder.Command.Edit;

public class PurchaseOrderHeaderEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? vendor_id { get; set; }
    public string? po_type { get; set; }
    public string? deleted_reason { get; set; }
    public string? canceled_reason { get; set; }
    public bool? is_complete { get; set; }
    public bool? is_canceled { get; set; }

    public List<PurchaseOrderLineEditCommand> purchase_order_line {  get; set; } = new List<PurchaseOrderLineEditCommand>();
}
