using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.ProductionOrder.Command.Delete;

public class ProductionOrderLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
