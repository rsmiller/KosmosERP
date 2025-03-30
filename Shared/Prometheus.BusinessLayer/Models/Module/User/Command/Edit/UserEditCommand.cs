using Prometheus.BusinessLayer.Models.Module.User.Command.Create;
using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.User.Command.Edit;

public class UserEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    [MaxLength(45)]
    public string? first_name { get; set; }
    [MaxLength(45)]
    public string? last_name { get; set; }
    [MaxLength(45)]
    public string? password { get; set; }
    [MaxLength(45)]
    public string? email { get; set; }
    public int? department { get; set; }
    public string? employee_number { get; set; }
    public bool? is_external_user { get; set; }
    public bool? is_deleted { get; set; }
    public bool? is_admin { get; set; }
    public bool? is_management { get; set; }
    public bool? is_guest { get; set; }
    public List<UserPermissionCreateCommand> new_permissions { get; set; } = new List<UserPermissionCreateCommand>();
    public List<UserPermissionEditCommand> edit_permissions { get; set; } = new List<UserPermissionEditCommand>();
    public List<int> delete_permissions { get; set; } = new List<int>();
}
