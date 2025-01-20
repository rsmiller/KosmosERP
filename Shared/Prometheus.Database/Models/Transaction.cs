using System.ComponentModel.DataAnnotations;

namespace Prometheus.Database.Models;

public class Transaction : BaseDatabaseModel
{
    [Required]
    public int product_id { get; set; }

    [Required]
    public int transaction_type { get; set; }

    [Required]
    public DateTime transaction_date { get; set; } = DateTime.Now;

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

    [Required]
    public decimal purchased_unit_cost { get; set; } = 0;

    [Required]
    public decimal sold_unit_price { get; set; } = 0;

    [MaxLength(50)]
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}
