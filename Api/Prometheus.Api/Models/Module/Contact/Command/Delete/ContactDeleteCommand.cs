using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Contact.Command.Delete;

public class ContactDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
