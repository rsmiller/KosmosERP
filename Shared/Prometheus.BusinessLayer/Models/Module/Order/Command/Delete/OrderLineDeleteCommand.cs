using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Order.Command.Delete;

public class OrderLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
