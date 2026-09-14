using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class UserSessionState
{
    [Required]
    [Key]
    public int id { get; set; }

    [Required]
    public int user_id { get; set; }

    [Required]
    [MaxLength(50)]
    public string session_id { get; set; }

    [Required]
    public DateTime session_expires { get; set; }

    [Required]
    public DateTime created_on { get; set; }
}
