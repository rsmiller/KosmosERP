using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.BOM.Dto;

public class BOMDto : BaseDto
{
    public int parent_product_id { get; set; }
    public int? bom_id { get; set; }
    public int quantity { get; set; }
    public string? instructions { get; set; }

    public List<BOMDto> child_boms { get; set; } = new List<BOMDto>();
}
