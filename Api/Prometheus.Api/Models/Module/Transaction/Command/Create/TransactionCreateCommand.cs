using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Transaction.Command.Create;

public class TransactionCreateCommand : DataCommand
{
    [Required]
    public int product_id { get; set; }

    [Required]
    public int transaction_type { get; set; }

    [Required]
    public DateTime transaction_date { get; set; }

    [Required]
    public int object_reference_id { get; set; }

    public int? object_sub_reference_id { get; set; }

    [Required]
    public int units_sold { get; set; } = 0;
    [Required]
    public int units_shipped { get; set; } = 0;
    [Required]
    public int units_purchased { get; set; } = 0;
    [Required]
    public int units_received { get; set; } = 0;
    public decimal purchased_unit_cost { get; set; } = 0;
    public decimal sold_unit_price { get; set; } = 0;
}
