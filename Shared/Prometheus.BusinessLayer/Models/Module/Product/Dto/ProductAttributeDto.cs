using Prometheus.Models;
using System;

namespace Prometheus.BusinessLayer.Models.Module.Product.Dto;

public class ProductAttributeDto : BaseDto
{
    public int id { get; set; }
    public int product_id { get; set; }
    public required string attribute_name { get; set; }
    public required string attribute_value { get; set; }
    public string? attribute_value2 { get; set; }
    public string? attribute_value3 { get; set; }
}
