using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;

public class ProductionOrderHeaderEditCommand : DataCommand
{
    public int? status_id { get; set; } = 0;
    public int? priority_id { get; set; } = 99;
    public DateOnly? planned_start_date { get; set; }
    public DateOnly? planned_complete_date { get; set; }
}
