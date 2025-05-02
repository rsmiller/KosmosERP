using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Edit;

public class VendorEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public string? vendor_name { get; set; }
    public string? vendor_description { get; set; }

    public int? address_id { get; set; }

    public string? phone { get; set; }
    public string? fax { get; set; }
    public string? general_email { get; set; }
    public string? website { get; set; }

    public string? category { get; set; }
    public bool? is_critial_vendor { get; set; }
    public DateTime? approved_on { get; set; }
    public int? approved_by { get; set; }
    public DateTime? audit_on { get; set; }
    public int? audit_by { get; set; }
    public DateTime? retired_on { get; set; }
    public int? retired_by { get; set; }
}
