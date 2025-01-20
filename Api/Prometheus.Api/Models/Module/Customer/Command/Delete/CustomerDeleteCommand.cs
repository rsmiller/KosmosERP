using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Customer.Command.Delete;

public class CustomerDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
