using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Product.Dto;

public class ProductDto : BaseDto
{
    public required int vendor_id { get; set; }
    public required string product_class { get; set; }
    public required string category { get; set; }
    public required string identifier1 { get; set; }
    public string? identifier2 { get; set; }
    public string? identifier3 { get; set; }
    public required string product_name { get; set; }
    public required string internal_description { get; set; }
    public string? external_description { get; set; }
    public int required_stock_level { get; set; } = 0;
    public int required_reorder_level { get; set; } = 0;
    public int required_min_order { get; set; } = 0;
    public decimal our_cost { get; set; }
    public decimal unit_cost { get; set; }
    public decimal sales_price { get; set; }
    public decimal list_price { get; set; }
    public string? rfid_id { get; set; }
    public bool is_taxable { get; set; } = true;
    public bool is_stock { get; set; } = false;
    public bool is_material { get; set; } = false;
    public bool is_rental_item { get; set; } = false;
    public bool is_sales_item { get; set; } = false;
    public bool is_labor { get; set; } = false;
    public bool is_shippable { get; set; } = true;
    public bool is_retired { get; set; } = true;
    public string? created_by_name { get; set; }
    public string? updated_by_name { get; set; }
    public DateTime? retired_on { get; set; }
    public string? retired_by_name { get; set; }

    public string? vendor_name { get; set; }

    public List<ProductAttributeDto> product_attributes { get; set; } = new List<ProductAttributeDto>();
}
