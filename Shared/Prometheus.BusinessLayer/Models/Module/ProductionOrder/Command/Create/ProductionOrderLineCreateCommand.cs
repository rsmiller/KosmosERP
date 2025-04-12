using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Create;

public class ProductionOrderLineCreateCommand : DataCommand
{
    [Required]
    public int production_order_header_id { get; set; }
    [Required]
    public int order_line_id { get; set; }
    [Required]
    public int quantity { get; set; }
    [Required]
    public DateTime? started_on { get; set; }
    [Required]
    public DateTime? completed_on { get; set; }
    [Required]
    public int status_id { get; set; }
    public string guid { get; set; }
}
