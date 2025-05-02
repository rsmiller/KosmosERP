
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class ProductionOrderLine : BaseDatabaseModel
{
    [Required]
    public int production_order_header_id { get; set; }
    [Required]
    public int order_line_id { get; set; }
    [Required]
    public int line_number { get; set; }
    [Required]
    public int quantity { get; set; }
    [Required]
    public int status_id { get; set; }
    public DateTime? started_on { get; set; }
    public DateTime? completed_on { get; set; }
    [Required]
    public bool is_complete { get; set; } = false;
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public OrderLine order_line { get; set; }
}
