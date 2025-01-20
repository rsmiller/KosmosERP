using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.User.Command.Create;

public class UserPermissionCreateCommand : DataCommand
{
    [Required]
    public int user_id { get; set; }
    [Required]
    public required string permission_name { get; set; }
    public bool read { get; set; } = false;
    public bool write { get; set; } = false;
    public bool edit { get; set; } = false;
    public bool delete { get; set; } = false;
}
