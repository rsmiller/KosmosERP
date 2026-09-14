using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Opportunity.Dto;

public class OpportunityListDto : BaseDto
{
    public string opportunity_name { get; set; }
    public int customer_id { get; set; }
    public int contact_id { get; set; }
    public decimal amount { get; set; }
    public string stage { get; set; }
    public int win_chance { get; set; }
    public DateOnly expected_close { get; set; }
    public string owner_id { get; set; }

    public string? customer_name { get; set; }
    public string? contact_name { get; set; }
    public string? stage_name { get; set; }
    public string? owner_name { get; set; }
}
