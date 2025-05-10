using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Order.Command.Create;

public class OrderHeaderCreateCommand : DataCommand
{
    public int customer_id { get; set; }

    [Required]
    public int ship_to_address_id { get; set; }

    [Required]
    public int shipping_method_id { get; set; }

    [Required]
    public int? pay_method_id { get; set; }

    public int? opportunity_id { get; set; }

    [Required]
    [MaxLength(2)]
    public string order_type { get; set; }

    [Required]
    public DateOnly order_date { get; set; }

    [Required]
    public DateOnly required_date { get; set; }

    public string? po_number { get; set; }
    public decimal shipping_cost { get; set; } = 0;

    public List<OrderLineCreateCommand> order_lines { get; set; } = new List<OrderLineCreateCommand>();


}
