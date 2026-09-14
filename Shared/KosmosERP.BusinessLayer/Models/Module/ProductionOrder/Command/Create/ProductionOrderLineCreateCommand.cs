using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Create;

public class ProductionOrderLineCreateCommand : DataCommand
{
    public int? production_order_header_id { get; set; }
    [Required]
    public int order_line_id { get; set; }
    [Required]
    public int quantity { get; set; }
    public DateTime? started_on { get; set; }
    public DateTime? completed_on { get; set; }
    [Required]
    public string status { get; set; }
    [Required]
    public int production_lead_minutes { get; set; }
    [Required]
    public int line_number { get; set; }
}
