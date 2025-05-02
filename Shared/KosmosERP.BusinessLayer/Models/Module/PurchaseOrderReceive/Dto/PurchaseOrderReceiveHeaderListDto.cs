using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Dto;

public class PurchaseOrderReceiveHeaderListDto : BaseDto
{
    public int purchase_order_id { get; set; }

    public int units_ordered { get; set; }

    public int units_received { get; set; }

    public bool is_complete { get; set; }

    public bool is_canceled { get; set; }

    public string? canceled_reason { get; set; }

    public DateTime? completed_on { get; set; }
    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }
}
