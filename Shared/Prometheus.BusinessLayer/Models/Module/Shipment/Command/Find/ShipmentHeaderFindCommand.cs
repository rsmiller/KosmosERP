using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Shipment.Command.Find;

public class ShipmentHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
