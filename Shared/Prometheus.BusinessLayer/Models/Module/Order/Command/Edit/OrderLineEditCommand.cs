using Microsoft.EntityFrameworkCore;
using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Order.Command.Edit;

public class OrderLineEditCommand : DataCommand
{
    public int? id { get; set; }
    public int? product_id { get; set; }
    public int? line_number { get; set; }
    public int? opportunity_line_id { get; set; }
    [MaxLength(250)]
    public string? line_description { get; set; }
    public int? quantity { get; set; }

    [Precision(14, 3)]
    public decimal? unit_price { get; set; }

    public List<OrderLineAttributeEditCommand> attributes { get; set; } = new List<OrderLineAttributeEditCommand>();
}
