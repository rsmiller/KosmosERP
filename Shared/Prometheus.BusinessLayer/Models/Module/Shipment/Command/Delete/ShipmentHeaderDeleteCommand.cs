using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Shipment.Command.Delete;

public class ShipmentHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
