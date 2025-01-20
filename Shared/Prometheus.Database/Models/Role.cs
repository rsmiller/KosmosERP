using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Prometheus.Database.Models;

public class Role : BaseDatabaseModel
{
    [Required]
    public string name { get; set; }

    [NotMapped]
    public List<RolePermission> role_permissions { get; set; } = new List<RolePermission>();
}
