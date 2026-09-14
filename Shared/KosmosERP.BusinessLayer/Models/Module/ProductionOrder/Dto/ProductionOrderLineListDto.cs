using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Dto;

public class ProductionOrderLineListDto : BaseDto
{
    public int production_order_header_id { get; set; }
    public int order_line_id { get; set; }
    public int quantity { get; set; }
    public string status { get; set; }
    public DateTime? started_on { get; set; }
    public DateTime? completed_on { get; set; }
    public bool is_complete { get; set; } = false;
    public int production_lead_minutes { get; set; }
    public string? status_name { get; set; }
}
