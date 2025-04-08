using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Order.Command.Edit;

public class OrderHeaderEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? customer_id { get; set; }
    public int? ship_to_address_id { get; set; }
    public int? shipping_method_id { get; set; }
    public int? pay_method_id { get; set; }
    public int? opportunity_id { get; set; }
    [MaxLength(2)]
    public string? order_type { get; set; }
    public DateOnly? order_date { get; set; }
    public DateOnly? required_date { get; set; }
    public string? po_number { get; set; }
    public decimal? tax { get; set; }
    public decimal? shipping_cost { get; set; }

    public List<OrderLineEditCommand> order_lines { get; set; } = new List<OrderLineEditCommand>();
}
