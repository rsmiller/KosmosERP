using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.BOM.Command.Create;

public class BOMCreateCommand : DataCommand
{
    public int parent_product_id { get; set; }
    public int product_id { get; set; }
    public int order_number { get; set; }
    public int quantity { get; set; }
    public string? instructions { get; set; }
    public int? parent_bom_id { get; set; }
    
    public BOMCreateCommand? child_bom { get; set; }
}
