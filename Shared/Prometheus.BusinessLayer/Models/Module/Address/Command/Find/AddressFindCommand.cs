using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Address.Command.Find;

public class AddressFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
