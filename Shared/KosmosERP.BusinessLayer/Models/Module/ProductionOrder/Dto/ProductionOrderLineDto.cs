using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Dto;

public class ProductionOrderLineDto : BaseDto
{
    public int production_order_header_id { get; set; }
    public int order_line_id { get; set; }
    public int quantity { get; set; }
    public int status_id { get; set; }
    public DateTime? started_on { get; set; }
    public DateTime? completed_on { get; set; }
    public bool is_complete { get; set; } = false;

    public OrderLineDto? order_line { get; set; }
}
