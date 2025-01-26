using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.PurchaseOrder.Command.Delete;

public class PurchaseOrderLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
