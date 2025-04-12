using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.State.Command.Delete;

public class ProductionOrderHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
