using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Shipment.Command.Delete;

public class ShipmentHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
