using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class Inventory
{
    [Required]
    public int id { get; set; }
    [Required]
    public int product_id { get; set; }
    [Required]
    public string product_name { get; set; }
    [Required]
    public int current_stock { get; set; } = 0;
    [Required]
    public int required_stock { get; set; } = 0;
    [Required]
    public int reorder_level { get; set; } = 0;
    [Required]
    public int on_hand { get; set; } = 0;
    [Required]
    public int reserved { get; set; } = 0;
    [Required]
    public int on_order { get; set; } = 0;
    [Required]
    public int to_order { get; set; } = 0;
    [Required]
    public int total_units_sold { get; set; } = 0;
    [Required]
    public int total_units_received { get; set; } = 0;
    [Required]
    public int total_units_shipped { get; set; } = 0;
    [Required]
    public int total_on_purchased { get; set; } = 0;
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public DateTime created_on { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime updated_on { get; set; } = DateTime.UtcNow;
}
