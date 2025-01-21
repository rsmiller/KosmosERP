using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Vendor.Command.Delete;

public class VendorDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
