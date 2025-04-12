using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Find;

public class ProductionOrderHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
