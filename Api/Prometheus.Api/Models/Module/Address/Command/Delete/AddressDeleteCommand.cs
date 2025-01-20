using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Address.Command.Delete;

public class AddressDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
