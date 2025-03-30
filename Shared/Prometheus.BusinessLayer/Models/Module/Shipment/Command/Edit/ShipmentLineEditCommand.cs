using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Shipment.Command.Create;

public class ShipmentLineEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? order_line_id { get; set; }
    public int? units_to_ship { get; set; } = 0;
    public int? units_shipped { get; set; } = 0;
    public bool? is_complete { get; set; }
    public bool? is_canceled { get; set; }
    [MaxLength(500)]
    public string? canceled_reason { get; set; }
}
