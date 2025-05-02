using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class OrderLine : BaseDatabaseModel
{
    [Required]
    public int order_header_id { get; set; }

    [Required]
    public int product_id { get; set; }

    [Required]
    public int line_number { get; set; }

    public int? opportunity_line_id { get; set; }

    [Required]
    [MaxLength(250)]
    public string line_description { get; set; }

    [Required]
    public int quantity { get; set; }

    [Precision(14, 3)]
    [Required]
    public decimal unit_price { get; set; }

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    public List<OrderLineAttribute> attributes { get; set; } = new List<OrderLineAttribute>();
}
