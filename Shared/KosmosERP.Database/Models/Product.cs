using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public partial class Product : BaseDatabaseModel
{
    [Required]
    public int vendor_id { get; set; }

    [MaxLength(50)]
    [Required]
    public string product_class { get; set; }

    [MaxLength(50)]
    [Required]
    public string category { get; set; }

    [MaxLength(50)]
    [Required]
    public string identifier1 { get; set; }

    [MaxLength(50)]
    public string? identifier2 { get; set; }

    [MaxLength(50)]
    public string? identifier3 { get; set; }

    [MaxLength(250)]
    [Required]
    public string product_name { get; set; }

    [MaxLength(1000)]
    [Required]
    public string internal_description { get; set; }

    [MaxLength(1000)]
    public string? external_description { get; set; }

    [Required]
    public int required_stock_level { get; set; } = 0;

    [Required]
    public int required_reorder_level { get; set; } = 0;

    [Required]
    public int required_min_order { get; set; } = 0;

    [Precision(14, 3)]
    [Required]
    public decimal our_cost { get; set; } = 0;

    [Precision(14, 3)]
    [Required]
    public decimal unit_cost { get; set; } = 0;

    [Precision(14, 3)]
    [Required]
    public decimal sales_price { get; set; } = 0;

    [Precision(14, 3)]
    [Required]
    public decimal list_price { get; set; } = 0;

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string? rfid_id { get; set; }

    [Required]
    public bool is_taxable { get; set; } = true;

    [Required]
    public bool is_stock { get; set; } = false;

    [Required]
    public bool is_material { get; set; } = false;

    [Required]
    public bool is_rental_item { get; set; } = false;

    [Required]
    public bool is_sales_item { get; set; } = false;

    [Required]
    public bool is_labor { get; set; } = false;

    [Required]
    public bool is_shippable { get; set; } = true;

    [Required]
    public bool is_retired { get; set; } = true;

    public DateTime? retired_on { get; set; }
    public int? retired_by { get; set; }

    [NotMapped]
    public List<ProductAttribute> attributes { get; set; } = new List<ProductAttribute>();
}
