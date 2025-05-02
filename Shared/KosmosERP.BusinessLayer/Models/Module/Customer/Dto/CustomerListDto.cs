using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Customer.Dto;

public class CustomerListDto : BaseDto
{
    public int customer_number { get; set; }
    public required string customer_name { get; set; }
    public string? customer_description { get; set; }
    public string phone { get; set; }
    public string? fax { get; set; }
    public string? general_email { get; set; }
    public string? website { get; set; }
    public string category { get; set; }
    public string guid { get; set; }
    public bool is_taxable { get; set; }
    public decimal tax_rate { get; set; }
}
