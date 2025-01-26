using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Shipment.Command.Find;

public class ShipmentHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
