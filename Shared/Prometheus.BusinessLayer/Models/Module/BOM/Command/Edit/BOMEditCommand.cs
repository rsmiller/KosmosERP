using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.BOM.Command.Edit;

public class BOMEditCommand : DataCommand
{
    public int? parent_product_id { get; set; }
    public int? bom_id { get; set; }
    public int? quantity { get; set; }
    public string? instructions { get; set; }
}
