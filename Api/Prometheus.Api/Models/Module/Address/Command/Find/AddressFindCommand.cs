using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Address.Command.Find;

public class AddressFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
