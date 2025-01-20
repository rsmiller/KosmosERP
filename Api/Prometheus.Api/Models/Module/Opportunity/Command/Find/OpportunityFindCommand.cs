using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Opportunity.Command.Find;

public class OpportunityFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
