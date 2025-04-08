using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Edit;

public class PurchaseOrderReceiveLineEditCommand : DataCommand
{
    public int? id { get; set; }
    public int? purchase_order_line_id { get; set; }
    public int? units_received { get; set; }
    public bool? is_complete { get; set; }
    public bool? is_canceled { get; set; }
    public string? canceled_reason { get; set; }
}
