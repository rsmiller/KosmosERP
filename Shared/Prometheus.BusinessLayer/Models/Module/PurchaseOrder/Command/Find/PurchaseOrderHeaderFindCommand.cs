using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.PurchaseOrder.Command.Find;

public class PurchaseOrderHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
