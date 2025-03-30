using Microsoft.EntityFrameworkCore;
using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.PurchaseOrder.Command.Create;

public class PurchaseOrderLineCreateCommand : DataCommand
{
    [Required]
    public int purchase_order_header_id { get; set; }

    [Required]
    public int product_id { get; set; }

    [Required]
    public int line_number { get; set; } = 1;

    [Required]
    public int quantity { get; set; } = 0;

    [Required]
    [MaxLength(1000)]
    public required string description { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal unit_price { get; set; } = 0;

    [Required]
    [Precision(14, 3)]
    public decimal tax { get; set; } = 0;

    [Required]
    public bool is_taxable { get; set; }
}
