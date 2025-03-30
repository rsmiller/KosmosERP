using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.State.Command.Find;

public class StateFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
