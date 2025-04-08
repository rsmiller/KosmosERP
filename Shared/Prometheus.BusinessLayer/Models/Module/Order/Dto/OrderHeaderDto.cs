using Prometheus.Models;
using Prometheus.BusinessLayer.Models.Module.Address.Dto;

namespace Prometheus.BusinessLayer.Models.Module.Order.Dto;

public class OrderHeaderDto : BaseDto
{
    public int order_number { get; set; }
    public int customer_id { get; set; }
    public int ship_to_address_id { get; set; }
    public int shipping_method_id { get; set; }
    public int? pay_method_id { get; set; }
    public int? opportunity_id { get; set; }
    public string order_type { get; set; }
    public int revision_number { get; set; }
    public DateOnly order_date { get; set; }
    public DateOnly required_date { get; set; }
    public string? po_number { get; set; }
    public decimal price { get; set; }
    public decimal tax { get; set; }
    public decimal shipping_cost { get; set; }
    public string guid { get; set; }
    public string? deleted_reason { get; set; }
    public string? canceled_reason { get; set; }
    public bool is_complete { get; set; } = false;
    public bool is_canceled { get; set; } = false;
    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }
    public AddressDto? ship_to_address { get; set; }
    public List<OrderLineDto> order_lines { get; set; } = new List<OrderLineDto>();
}
