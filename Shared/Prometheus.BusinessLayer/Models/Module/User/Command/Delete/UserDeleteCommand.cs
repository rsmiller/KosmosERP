using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.User.Command.Delete;

public class UserDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
