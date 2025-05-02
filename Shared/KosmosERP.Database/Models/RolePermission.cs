using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace KosmosERP.Database.Models;

public class RolePermission : BaseDatabaseModel
{
    [Required]
    public int role_id { get; set; }

    [Required]
    public int module_permission_id { get; set; }

    [NotMapped]
    public ModulePermission module_permission { get; set; }
}
