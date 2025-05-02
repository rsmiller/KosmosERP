using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Create;

public class ShipmentLineCreateCommand : DataCommand
{
    [Required]
    public int shipment_header_id { get; set; }

    [Required]
    public int order_line_id { get; set; }

    [Required]
    public int units_to_ship { get; set; } = 0;

    [Required]
    public int units_shipped { get; set; } = 0;
}
