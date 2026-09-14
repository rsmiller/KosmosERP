using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class ModulePermission : BaseDatabaseModel
{
    [Required]
    [MaxLength(255)]
    public string module_id { get; set; }

    [Required]
    public string module_name { get; set; }

    [Required]
    public string permission_name { get; set; }

    [Required]
    [MaxLength(255)]
    public string internal_permission_name { get; set; }

    [Required]
    public bool read { get; set; } = false;

    [Required]
    public bool write { get; set; } = false;

    [Required]
    public bool edit { get; set; } = false;

    [Required]
    public bool delete { get; set; } = false;

    [Required]
    public bool requires_admin { get; set; } = false;

    [Required]
    public bool requires_management { get; set; } = false;

    [Required]
    public bool requires_guest { get; set; } = false;

    [Required]
    public bool is_active { get; set; } = true;
}
