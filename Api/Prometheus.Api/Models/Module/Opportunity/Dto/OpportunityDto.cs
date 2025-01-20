using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Opportunity.Dto; 

public class OpportunityDto : BaseDto
{
    public required string opportunity_name { get; set; }
    public int customer_id { get; set; }
    public int contact_id { get; set; }
    public decimal amount { get; set; }
    public string stage { get; set; }
    public int win_chance { get; set; }
    public DateOnly expected_close { get; set; }
    public int owner_id { get; set; }
}
