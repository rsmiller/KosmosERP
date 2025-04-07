using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Order.Command.Edit;

public class OrderLineEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
