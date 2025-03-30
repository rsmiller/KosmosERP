using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Vendor.Command.Find;

public class VendorFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
