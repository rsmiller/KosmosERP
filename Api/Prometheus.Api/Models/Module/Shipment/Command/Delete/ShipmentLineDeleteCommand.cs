using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Shipment.Command.Delete;

public class ShipmentLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
