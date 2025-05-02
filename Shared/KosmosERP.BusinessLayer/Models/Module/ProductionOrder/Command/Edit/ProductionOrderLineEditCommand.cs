using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;

public class ProductionOrderLineEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? production_order_header_id { get; set; }
    public int? quantity { get; set; }
    public DateTime? started_on { get; set; }
    public DateTime? completed_on { get; set; }
    public int? status_id { get; set; }
    public int? line_number { get; set; }
    public bool? is_complete { get; set; }
}
