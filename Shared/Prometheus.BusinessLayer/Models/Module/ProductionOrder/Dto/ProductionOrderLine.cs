using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.ProductionOrder.Dto;

public class ProductionOrderLine : BaseDto
{
    public int production_order_header_id { get; set; }
    public int order_line_id { get; set; }
    public int quantity { get; set; }
    public int status_id { get; set; }
    public DateTime? started_on { get; set; }
    public DateTime? completed_on { get; set; }
    public bool is_complete { get; set; } = false;
    public string guid { get; set; }
}
