using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.User.Command.Edit;

public class UserPermissionEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public string? permission_name { get; set; }
    public bool? read { get; set; }
    public bool? write { get; set; }
    public bool? edit { get; set; }
    public bool? delete { get; set; }
}
