using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Find;

public class PurchaseOrderReceiveLineFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
