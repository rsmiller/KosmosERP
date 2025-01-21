using Prometheus.Api.Models.Module.Address.Command.Create;
using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Vendor.Command.Create;

public class VendorCreateCommand : DataCommand
{
    [Required]
    public required string vendor_name { get; set; }
    public string? vendor_description { get; set; }

    [Required]
    public AddressCreateCommand address { get; set; }

    [Required]
    public string phone { get; set; }
    public string? fax { get; set; }
    public string? general_email { get; set; }
    public string? website { get; set; }

    [Required]
    public required string category { get; set; }
    public bool is_critial_vendor { get; set; } = false;
    public DateTime? approved_on { get; set; }
    public int? approved_by { get; set; }
    public DateTime? audit_on { get; set; }
    public int? audit_by { get; set; }
    public DateTime? retired_on { get; set; }
    public int? retired_by { get; set; }
}
