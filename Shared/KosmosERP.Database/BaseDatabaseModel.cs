
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database;
public partial class BaseDatabaseModel
{
    [Required]
    [Key]
    public int id { get; set; }

    [Required]
    public bool is_deleted { get; set; } = false;

    [Required]
    public DateTime created_on { get; set; }

    [Required]
    public string created_on_timezone { get; set; }

    [Required]
    public string created_on_string { get; set; }

    [Required]
    public int created_by { get; set; }

    public DateTime? updated_on { get; set; }
    public int? updated_by { get; set; }

    public string updated_on_timezone { get; set; }
    public string updated_on_string { get; set; }

    public DateTime? deleted_on { get; set; }

    public int? deleted_by { get; set; }

    public string deleted_on_timezone { get; set; }
    public string deleted_on_string { get; set; }
}

