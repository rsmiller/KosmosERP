using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Product.Command.Edit;

public class ProductEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? vendor_id { get; set; }
    [MaxLength(50)]
    public string? product_class { get; set; }
    [MaxLength(250)]
    public string? product_name { get; set; }
    [MaxLength(50)]
    public string? category { get; set; }
    [MaxLength(50)]
    public string? identifier1 { get; set; }
    [MaxLength(50)]
    public string? identifier2 { get; set; }
    [MaxLength(50)]
    public string? identifier3 { get; set; }
    [MaxLength(1000)]
    public string? internal_description { get; set; }
    [MaxLength(1000)]
    public string? external_description { get; set; }
    public int? required_stock_level { get; set; }
    public int? required_reorder_level { get; set; }
    public int? required_min_order { get; set; }
    [Precision(14, 3)]
    public decimal? our_cost { get; set; }
    [Precision(14, 3)]
    public decimal? unit_cost { get; set; }
    [Precision(14, 3)]
    public decimal? sales_price { get; set; }
    [Precision(14, 3)]
    public decimal? list_price { get; set; }
    [MaxLength(50)]
    public string? rfid_id { get; set; }

    public bool? is_taxable { get; set; }
    public bool? is_stock { get; set; }
    public bool? is_material { get; set; }
    public bool? is_rental_item { get; set; }
    public bool? is_sales_item { get; set; }
    public bool? is_labor { get; set; }
    public bool? is_shippable { get; set; }
    public bool? is_retired { get; set; }
}
