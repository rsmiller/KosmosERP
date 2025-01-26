using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Shipment.Command.Create;

public class ShipmentHeaderCreateCommand : DataCommand
{
    [Required]
    public int order_id { get; set; }

    [Required]
    public int address_id { get; set; }
    [Required]
    public string ship_via { get; set; }
    public string? ship_attn { get; set; }
    public string? freight_carrier { get; set; }
    [Required]
    public decimal freight_charge_amount { get; set; }
    [Required]
    public decimal tax { get; set; } = 0;

    public List<ShipmentLineCreateCommand> shipment_lines { get; set; } = new List<ShipmentLineCreateCommand>();
}
