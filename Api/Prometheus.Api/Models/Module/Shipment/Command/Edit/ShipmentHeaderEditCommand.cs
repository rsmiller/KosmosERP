using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Shipment.Command.Create;

public class ShipmentHeaderEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? order_id { get; set; }
    public int? address_id { get; set; }
    public string? ship_via { get; set; }
    public string? ship_attn { get; set; }
    public string? freight_carrier { get; set; }
    public decimal? freight_charge_amount { get; set; }
    public decimal? tax { get; set; }
    public bool? is_complete { get; set; }
    public bool? is_canceled { get; set; }
    [MaxLength(500)]
    public string? canceled_reason { get; set; }
}
