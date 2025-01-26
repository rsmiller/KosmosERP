using Prometheus.Models;

namespace Prometheus.Api.Models.Module.PurchaseOrder.Dto;

public class PurchaseOrderLineDto : BaseDto
{
    public int purchase_order_header_id { get; set; }
    public int product_id { get; set; }
    public int revision_number { get; set; }
    public int line_number { get; set; }
    public int quantity { get; set; }
    public required string description { get; set; }
    public decimal unit_price { get; set; }
    public decimal tax { get; set; }
    public bool is_taxable { get; set; }
    public bool is_complete { get; set; }
    public bool is_canceled { get; set; }
    public string guid { get; set; }
}
