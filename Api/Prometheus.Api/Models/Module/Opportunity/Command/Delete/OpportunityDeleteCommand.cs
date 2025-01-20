using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Opportunity.Command.Delete;

public class OpportunityDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
