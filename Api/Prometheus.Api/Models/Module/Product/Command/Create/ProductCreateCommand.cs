using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Product.Command.Create;

public class ProductCreateCommand : DataCommand
{
    [Required]
    public int vendor_id { get; set; }
    [Required]
    [MaxLength(50)]
    public required string product_class { get; set; }

    [Required]
    [MaxLength(50)]
    public required string category { get; set; }

    [Required]
    [MaxLength(50)]
    public required string identifier1 { get; set; }

    [MaxLength(50)]
    public string? identifier2 { get; set; }
    [MaxLength(50)]
    public string? identifier3 { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string internal_description { get; set; }

    [MaxLength(1000)]
    public string? external_description { get; set; }

    public int required_stock_level { get; set; } = 0;
    public int required_reorder_level { get; set; } = 0;
    public int required_min_order { get; set; } = 0;
    public decimal our_cost { get; set; } = 0;
    public decimal unit_cost { get; set; } = 0;
    public decimal sales_price { get; set; } = 0;
    public decimal list_price { get; set; } = 0;
    public string guid { get; set; } = Guid.NewGuid().ToString();
    public string? rfid_id { get; set; }
    public bool is_taxable { get; set; } = true;
    public bool is_stock { get; set; } = false;
    public bool is_material { get; set; } = false;
    public bool is_rental_item { get; set; } = false;
    public bool is_sales_item { get; set; } = false;
    public bool is_labor { get; set; } = false;
    public bool is_shippable { get; set; } = true;
    public bool is_retired { get; set; } = true;

    public List<ProductAttributeCreateCommand> product_attributes { get; set; } = new List<ProductAttributeCreateCommand>();
}
