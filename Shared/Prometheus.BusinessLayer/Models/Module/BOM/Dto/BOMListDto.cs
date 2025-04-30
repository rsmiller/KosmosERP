using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.BOM.Dto;

public class BOMListDto : BaseDto
{
    public int parent_product_id { get; set; }
    public int? parent_bom_id { get; set; }
    public int quantity { get; set; }
    public string? instructions { get; set; }
}
