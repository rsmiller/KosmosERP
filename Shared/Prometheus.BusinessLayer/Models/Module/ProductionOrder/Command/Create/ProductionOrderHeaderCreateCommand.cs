using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Create;

public class ProductionOrderHeaderCreateCommand : DataCommand
{
    [Required]
    public int order_header_id { get; set; }
    [Required]
    public int status_id { get; set; } = 0;
    [Required]
    public int priority_id { get; set; } = 99;
    [Required]
    public DateOnly planned_start_date { get; set; }
    [Required]
    public DateOnly planned_complete_date { get; set; }
    public string guid { get; set; }
}
