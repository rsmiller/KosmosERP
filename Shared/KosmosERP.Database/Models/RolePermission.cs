using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class RolePermission : BaseDatabaseModel
{
    [Required]
    public int role_id { get; set; }

    [Required]
    [MaxLength(50)]
    public string module_id { get; set; }

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
}
