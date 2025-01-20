using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Opportunity.Command.Create;

public class OpportunityCreateCommand : DataCommand
{
    [Required]
    public string opportunity_name { get; set; }

    [Required]
    public int customer_id { get; set; }

    [Required]
    public int contact_id { get; set; }

    [Required]
    public decimal amount { get; set; }

    [Required]
    public string stage { get; set; }

    [Required]
    public int win_chance { get; set; }

    [Required]
    public DateOnly expected_close { get; set; }
}
