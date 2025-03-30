using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Address.Command.Delete;

public class AddressDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
