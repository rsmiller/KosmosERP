using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Opportunity.Dto;

public class OpportunityLineDto : BaseDto
{
    public int opportunity_id { get; set; }
    public int? product_id { get; set; }
    public string description { get; set; }
    public int line_number { get; set; }
    public int quantity { get; set; }
    public decimal unit_price { get; set; }
    public string guid { get; set; }

    // Navigation properties for related data
    public string? product_name { get; set; }
    public string? identifier1 { get; set; }
    public string? opportunity_name { get; set; }
} 