using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Opportunity.Command.Find;

public class OpportunityFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
