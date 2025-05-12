using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Shipment.Dto;

public class ShipmentHeaderDto : BaseDto
{
    public int order_header_id { get; set; }
    public int shipment_number { get; set; }
    public int address_id { get; set; }
    public int units_to_ship { get; set; }
    public int units_shipped { get; set; }
    public bool is_complete { get; set; }
    public bool is_canceled { get; set; }
    public string ship_via { get; set; }
    public string? ship_attn { get; set; }
    public string? freight_carrier { get; set; }
    public decimal freight_charge_amount { get; set; }
    public decimal tax { get; set; }
    public DateTime? completed_on { get; set; }
    public int? completed_by { get; set; }
    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }
    public string? canceled_reason { get; set; }
    public string guid { get; set; }

    public List<ShipmentLineDto> shipment_lines { get; set; } = new List<ShipmentLineDto>();
}
