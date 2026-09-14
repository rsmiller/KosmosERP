using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Create;

public class ProductionOrderHeaderCreateCommand : DataCommand
{
    [Required]
    public int order_header_id { get; set; }
    [Required]
    public string status { get; set; }
    [Required]
    public int priority_id { get; set; } = 99;
    public DateOnly? planned_start_date { get; set; }
    public DateOnly? planned_complete_date { get; set; }

    [Required]
    public List<ProductionOrderLineCreateCommand> production_order_lines { get; set; } = new List<ProductionOrderLineCreateCommand>();
}
