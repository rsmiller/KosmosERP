using KosmosERP.BusinessLayer.Models.Module.Address.Dto;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Vendor.Dto;

public class VendorDto : BaseDto
{
    public required int vendor_number { get; set; }
    public required string vendor_name { get; set; }
    public string? vendor_description { get; set; }
    public int address_id { get; set; }
    public required AddressDto address { get; set; }
    public required string phone { get; set; }
    public string? fax { get; set; }
    public string? general_email { get; set; }
    public string? website { get; set; }
    public required string category { get; set; }
    public bool is_critial_vendor { get; set; }
    public DateTime? approved_on { get; set; }
    public int? approved_by { get; set; }
    public DateTime? audit_on { get; set; }
    public int? audit_by { get; set; }
    public DateTime? retired_on { get; set; }
    public int? retired_by { get; set; }

}
