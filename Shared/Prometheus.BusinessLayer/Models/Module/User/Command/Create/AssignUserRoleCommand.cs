using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.User.Command.Create;

public class AssignUserRoleCommand : DataCommand
{
    [Required]
    public int user_id { get; set; }
    [Required]
    public int role_id { get; set; }
}
