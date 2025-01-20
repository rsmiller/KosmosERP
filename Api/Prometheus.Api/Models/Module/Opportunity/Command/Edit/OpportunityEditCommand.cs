using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Opportunity.Command.Edit;

public class OpportunityEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public string? opportunity_name { get; set; }
    public int? customer_id { get; set; }
    public int? contact_id { get; set; }
    public decimal? amount { get; set; }
    public string? stage { get; set; }
    public int? win_chance { get; set; }
    public DateOnly? expected_close { get; set; }
    public int? owner_id { get; set; }
}
