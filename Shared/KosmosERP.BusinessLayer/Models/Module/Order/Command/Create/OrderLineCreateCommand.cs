using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Order.Command.Create;

public class OrderLineCreateCommand : DataCommand
{
    public int? order_header_id { get; set; }

    [Required]
    public int product_id { get; set; }

    [Required]
    public int line_number { get; set; }

    public int? opportunity_line_id { get; set; }

    [Required]
    public string line_description { get; set; }

    [Required]
    public int quantity { get; set; }

    [Precision(14, 3)]
    [Required]
    public decimal unit_price { get; set; }

    public List<OrderLineAttributeCreateCommand> attributes { get; set; } = new List<OrderLineAttributeCreateCommand>();
}
