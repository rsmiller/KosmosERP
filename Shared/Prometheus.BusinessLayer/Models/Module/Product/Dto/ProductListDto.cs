using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Product.Dto;

public class ProductListDto : BaseDto
{
    public required int vendor_id { get; set; }
    public required string vendor_name { get; set; }
    public required string product_class { get; set; }
    public required string category { get; set; }
    public required string identifier1 { get; set; }
    public string? identifier2 { get; set; }
    public string? identifier3 { get; set; }
    public required string product_name { get; set; }
    public required string internal_description { get; set; }
    public string? external_description { get; set; }
    public decimal sales_price { get; set; }
    public decimal list_price { get; set; }
    public decimal unit_cost { get; set; }
    public bool is_sales_item { get; set; }
}
