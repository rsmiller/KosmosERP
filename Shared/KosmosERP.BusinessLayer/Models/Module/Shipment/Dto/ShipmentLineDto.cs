
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Shipment.Dto;

public class ShipmentLineDto : BaseDto
{
    public int shipment_header_id { get; set; }
    public int order_line_id { get; set; }
    public int units_to_ship { get; set; }
    public int units_shipped { get; set; }
    public bool is_complete { get; set; }
    public bool is_canceled { get; set; }
    public DateTime? completed_on { get; set; }
    public int? completed_by { get; set; }
    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }
    public string? canceled_reason { get; set; }
    public string guid { get; set; }
}
