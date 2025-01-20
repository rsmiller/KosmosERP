using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Database.Models;

public class OpportunityLine : BaseDatabaseModel
{
    [Required]
    public int opportunity_id { get; set; }

    public int? product_id { get; set; }

    [Required]
    [MaxLength(250)]
    public string description { get; set; }

    [Required]
    public int line_number { get; set; }

    [Required]
    public int quantity { get; set; }

    [Precision(14, 3)]
    [Required]
    public decimal unit_price { get; set; }

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}
