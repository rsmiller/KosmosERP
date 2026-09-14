using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public partial class User : BaseDatabaseModel
{
    [Required]
    [MaxLength(50)]
    public string first_name { get; set; }

    [Required]
    [MaxLength(50)]
    public string last_name { get; set; }

    [MaxLength(50)]
    public string? email { get; set; }

    [Required]
    [MaxLength(50)]
    public string username { get; set; }

    [Required]
    [MaxLength(250)]
    public string password { get; set; }

    [Required]
    [MaxLength(250)]
    public string password_salt { get; set; }

    [MaxLength(10)]
    public string employee_number { get; set; }

    public string? department { get; set; }

    [Required]
    public int login_attempt{ get; set; } = 0;

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public bool is_external_user { get; set; } = false;

    public string external_id { get; set; 
    }
    [Required]
    public bool is_admin { get; set; } = false;

    [Required]
    public bool is_management { get; set; } = false;

    [Required]
    public bool is_guest { get; set; } = false;

    [Required]
    public bool is_disabled { get; set; } = false;

    [NotMapped]
    public List<UserRole> roles { get; set; } = new List<UserRole>();

    [NotMapped]
    public List<UserSessionState> sessions { get; set; } = new List<UserSessionState>();
}
