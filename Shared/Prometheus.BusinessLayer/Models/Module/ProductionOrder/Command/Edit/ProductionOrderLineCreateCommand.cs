using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;

public class ProductionOrderLineEditCommand : DataCommand
{
    public int? quantity { get; set; }
    public DateTime? started_on { get; set; }
    public DateTime? completed_on { get; set; }
    public int? status_id { get; set; }
}
