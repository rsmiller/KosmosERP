using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.User.Command.Edit;

public class RoleModulePermissionEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public string? module_id { get; set; }
    public bool? read { get; set; }
    public bool? write { get; set; }
    public bool? edit { get; set; }
    public bool? delete { get; set; }
}
