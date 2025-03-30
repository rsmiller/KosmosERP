using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.User.Command.Create;

public class UserCreateCommand : DataCommand
{
    [Required]
    [MaxLength(45)]
    public required string first_name { get; set; }
    [Required]
    [MaxLength(45)]
    public required string last_name { get; set; }
    [Required]
    [MaxLength(45)]
    public required string password { get; set; }
    [MaxLength(45)]
    public string? email { get; set; }
    [Required]
    [MaxLength(45)]
    public string? username { get; set; }
    public int? department { get; set; }
    public string? employee_number { get; set; }
    public bool is_external_user { get; set; } = false;
    public bool is_deleted { get; set; } = false;
    public bool is_admin { get; set; } = false;
    public bool is_management { get; set; } = false;
    public bool is_guest { get; set; } = false;
    public List<UserPermissionCreateCommand> new_permissions { get; set; } = new List<UserPermissionCreateCommand>();
}
