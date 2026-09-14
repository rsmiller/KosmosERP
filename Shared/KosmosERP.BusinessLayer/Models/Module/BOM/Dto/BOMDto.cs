using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.BOM.Dto;

public class BOMDto : BaseDto
{
    public int parent_product_id { get; set; }
    public int product_id { get; set; }
    public int? bom_id { get; set; }
    public int order_number { get; set; }
    public int quantity { get; set; }
    public string? instructions { get; set; }

    public string? product_name { get; set; }
    public List<BOMDto> child_boms { get; set; } = new List<BOMDto>();
}
