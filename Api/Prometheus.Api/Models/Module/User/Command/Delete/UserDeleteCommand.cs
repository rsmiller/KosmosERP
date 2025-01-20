using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.User.Command.Delete;

public class UserDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
