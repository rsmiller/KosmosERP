using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Order.Dto;

public class OrderLineAttributeDto : BaseDto
{
    public int order_line_id { get; set; }
    public string attribute_name { get; set; }
    public string attribute_value { get; set; }
    public string? attribute_value2 { get; set; }
    public string? attribute_value3 { get; set; }
    public string guid { get; set; }
}
