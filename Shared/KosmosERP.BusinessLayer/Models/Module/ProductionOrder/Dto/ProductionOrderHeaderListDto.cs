using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Dto;

public class ProductionOrderHeaderListDto : BaseDto
{
    public int order_header_id { get; set; }
    public string status { get; set; }
    public int priority_id { get; set; } = 99;
    public DateOnly? planned_start_date { get; set; }
    public DateOnly? planned_complete_date { get; set; }
    public DateOnly? actual_completed_on { get; set; }
    public bool is_complete { get; set; } = false;
    public int production_lead_minutes { get; set; }
    
    public int order_number { get; set; }
    public string? status_name { get; set; }
}
