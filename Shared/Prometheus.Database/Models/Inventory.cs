using System.ComponentModel.DataAnnotations;

namespace Prometheus.Database.Models;

public class Inventory
{
    [Required]
    public int id { get; set; }
    [Required]
    public int product_id { get; set; }
    [Required]
    public int current_stock { get; set; } = 0;
    [Required]
    public int on_hand { get; set; } = 0;
    [Required]
    public int reserved { get; set; } = 0;
    [Required]
    public int on_order { get; set; } = 0;
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public DateTime created_on { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime updated_on { get; set; } = DateTime.UtcNow;
}
