using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.BOM.Command.Create;

public class BOMCreateCommand : DataCommand
{
    public int parent_product_id { get; set; }
    public int quantity { get; set; }
    public string? instructions { get; set; }

    public BOMCreateCommand? child_bom { get; set; }
}
