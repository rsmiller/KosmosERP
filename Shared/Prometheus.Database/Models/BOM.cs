
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class BOM : BaseDatabaseModel
{
    public int parent_product_id { get; set; }
    public int? parent_bom_id { get; set; }
    public int quantity { get; set; }
    public string? instructions { get; set; }
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public Product parent_product { get; set; }
}
