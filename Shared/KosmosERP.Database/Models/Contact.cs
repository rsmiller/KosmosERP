using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class Contact : BaseDatabaseModel
{
    [Required]
    public int customer_id { get; set; }

    [Required]
    [MaxLength(100)]
    public string first_name { get; set; }

    [Required]
    [MaxLength(100)]
    public string last_name { get; set; }

    [MaxLength(100)]
    public string? title { get; set; }

    [MaxLength(200)]
    public string? email { get; set; }

    [MaxLength(50)]
    public string? phone { get; set; }

    [MaxLength(50)]
    public string? cell_phone { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}
