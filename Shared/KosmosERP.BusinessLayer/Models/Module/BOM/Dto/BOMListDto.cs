using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.BOM.Dto;

public class BOMListDto : BaseDto
{
    public int parent_product_id { get; set; }
    public int? parent_bom_id { get; set; }
    public int product_id { get; set; }
    public int order_number { get; set; }
    public int quantity { get; set; }
    public string? instructions { get; set; }

    public string? product_name { get; set; }
}
