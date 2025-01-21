using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Vendor.Command.Find;

public class VendorFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
