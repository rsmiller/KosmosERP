using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.BOM.Command.Edit;

public class BOMEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? parent_product_id { get; set; }
    public int? parent_bom_id { get; set; }
    public int? quantity { get; set; }
    public string? instructions { get; set; }
}
