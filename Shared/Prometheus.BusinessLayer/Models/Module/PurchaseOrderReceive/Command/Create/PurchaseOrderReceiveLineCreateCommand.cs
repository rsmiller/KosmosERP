using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;

public class PurchaseOrderReceiveLineCreateCommand : DataCommand
{
    public int purchase_order_line_id { get; set; }
    public int units_received { get; set; }
}
