using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class OrderHeader : BaseDatabaseModel
{
    [Required]
    public int order_number { get; set; }

    [Required]
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
    public int revision_number { get; set; } = 1;

    [Required]
    public DateOnly order_date {  get; set; }

    [Required]
    public DateOnly required_date { get; set; }

    public string? po_number { get; set; }


    [Precision(14, 3)]
    [Required]
    public decimal price { get; set; }

    [Precision(14, 3)]
    [Required]
    public decimal tax { get; set; } = 0;

    [Precision(14, 3)]
    [Required]
    public decimal shipping_cost { get; set; } = 0;

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(300)]
    public string? deleted_reason { get; set; }

    [MaxLength(500)]
    public string? canceled_reason { get; set; }

    [Required]
    public bool is_complete { get; set; } = false;

    [Required]
    public bool is_canceled { get; set; } = false;

    [NotMapped]
    public List<OrderLine> order_lines { get; set; } = new List<OrderLine>();

    public DateTime? canceled_on { get; set; }
    public int? canceled_by { get; set; }

    [NotMapped]
    public Address ship_to_address { get; set; }
}
