using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Vendor.Command.Delete;

public class VendorDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
