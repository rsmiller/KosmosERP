using Prometheus.Models;

namespace Prometheus.Api.Models.Module.State.Command.Find;

public class StateFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
