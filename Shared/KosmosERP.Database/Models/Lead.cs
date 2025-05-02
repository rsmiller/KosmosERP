using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class Lead : BaseDatabaseModel
{
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
    public string company_name { get; set; }

    [Required]
    public string lead_stage { get; set; }

    public string? time_zone { get; set; }
    public string? address_line1 { get; set; }
    public string? address_line2 { get; set; }
    public string? city { get; set; }
    public string? state { get; set; }
    public string? zip { get; set; }
    public string? country { get; set; }

    [Required]
    public bool is_converted { get; set; } = false;

    public int? converted_customer_id { get; set; }
    public int? converted_contact_id { get; set; }

    [Required]
    public int owner_id { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}
