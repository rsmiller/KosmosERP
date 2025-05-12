using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

// TODO: If order line quantities are updated the total of units_to_ship need to modified as well. Shipment module will need to subscribe to changes to the order line
// TODO: When an order is shipped we must check units_to_ship and ordered
public class ShipmentHeader : BaseDatabaseModel
{
    [Required]
    public int order_header_id { get; set; }

    [Required]
    public int shipment_number { get; set; }

    [Required]
    public int address_id { get; set; }

    [Required]
    public int revision_number { get; set; } = 1;

    [Required]
    public int units_to_ship { get; set; } = 0;

    [Required]
    public int units_shipped { get; set; } = 0;

    [Required]
    public bool is_complete { get; set; } = false;

    [Required]
    public bool is_canceled { get; set; } = false;

    [Required]
    [MaxLength(150)]
    public string ship_via { get; set; }

    [MaxLength(150)]
    public string? ship_attn { get; set; }

    [MaxLength(150)]
    public string? freight_carrier { get; set; }

    [Precision(14, 3)]
    [Required]
    public decimal freight_charge_amount { get; set; } = 0;

    [Precision(14, 3)]
    [Required]
    public decimal tax { get; set; } = 0;

    public DateTime? completed_on { get; set; }
    public int? completed_by { get; set; }
    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }

    [MaxLength(500)]
    public string? canceled_reason { get; set; }

    [MaxLength(50)]
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<ShipmentLine> shipment_lines { get; set; } = new List<ShipmentLine>();

    [NotMapped]
    public Address address { get; set; }
}
