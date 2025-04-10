using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Product.Command.Edit;

public class ProductAttributeEditCommand : DataCommand
{
    public int id { get; set; }
    public int? product_id { get; set; }
    public string? attribute_name { get; set; }
    public string? attribute_value { get; set; }
    public string? attribute_value2 { get; set; }
    public string? attribute_value3 { get; set; }
}
