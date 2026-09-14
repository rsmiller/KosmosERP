
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace KosmosERP.Database.Models;

public class UserRole : BaseDatabaseModel
{
    [Required]
    public int user_id { get; set; }

    [Required]
    public int role_id { get; set; }

    [NotMapped]
    public Role role { get; set; }
}
