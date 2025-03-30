using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Product.Command.Create;

public class ProductAttributeCreateCommand
{
    [Required]
    public required string attribute_name { get; set; }
    [Required]
    public required string attribute_value { get; set; }
    public string? attribute_value2 { get; set; }
    public string? attribute_value3 { get; set; }
}
