
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class ProductionOrderLine : BaseDatabaseModel
{
    public int production_order_header_id { get; set; }
    public int order_line_id { get; set; }
    public int quantity { get; set; }
    public int status_id { get; set; }
    public DateTime? started_on { get; set; }
    public DateTime? completed_on { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public OrderLine order_line { get; set; }
}
