using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class ProductionOrderHeader : BaseDatabaseModel
{
    public int order_header_id { get; set; }
    public int status_id { get; set; }
    public int priority_id { get; set; } = 99;
    public DateOnly planned_start_date { get; set; }
    public DateOnly planned_complete_date { get; set; }
    public DateOnly? actual_completed_on { get; set; }
    public bool is_complete { get; set; } = false;
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public OrderHeader order_header { get; set; }

    [NotMapped]
    public List<ProductionOrderLine> production_order_lines { get; set; } = new List<ProductionOrderLine>();
}
