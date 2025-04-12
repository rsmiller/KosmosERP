using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Lead.Command.Find;

public class LeadFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
