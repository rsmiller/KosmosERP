using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Create;

public class PurchaseOrderReceiveLineCreateCommand : DataCommand
{
    public int? purchase_order_receive_header_id { get; set; }
    public int purchase_order_line_id { get; set; }
    public int units_received { get; set; }
}
