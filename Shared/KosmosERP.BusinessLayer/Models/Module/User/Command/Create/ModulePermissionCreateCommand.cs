using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.User.Command.Create;

public class RoleModulePermissionCreateCommand : DataCommand
{
    [Required]
    public string module_id { get; set; }
    [Required]
    public int role_id { get; set; }
    public bool read { get; set; } = false;
    public bool write { get; set; } = false;
    public bool edit { get; set; } = false;
    public bool delete { get; set; } = false;
}
