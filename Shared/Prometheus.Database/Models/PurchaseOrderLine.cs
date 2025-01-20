using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;
public class PurchaseOrderLine : BaseDatabaseModel
{
    [Required]
    public int purchase_order_header_id { get; set; }

    [Required]
    public int? product_id { get; set; }

    [Required]
    public int revision_number { get; set; } = 1;

    [Required]
    public int line_number { get; set; } = 1;

    [Required]
    public int quantity { get; set; } = 0;

    [Required]
    [MaxLength(1000)]
    public string description { get; set; }

    [Required]
    [Precision(14, 3)]
    public decimal unit_price { get; set; } = 0;

    [Required]
    [Precision(14, 3)]
    public decimal tax { get; set; } = 0;

    [Required]
    public bool is_taxable { get; set; } = true;

    [Required]
    public bool is_complete { get; set; } = false;

    [Required]
    public bool is_canceled { get; set; } = false;

    [MaxLength(50)]
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public Product product { get; set; }
}
