using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Delete;

public class VendorDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
