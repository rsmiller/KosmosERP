using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.State.Command.Delete;

public class ProductionOrderLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
