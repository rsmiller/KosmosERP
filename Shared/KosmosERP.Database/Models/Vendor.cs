using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class Vendor : BaseDatabaseModel
{
    [Required]
    public int vendor_number { get; set; }

    [Required]
    [MaxLength(150)]
    public string vendor_name { get; set; }

    [MaxLength(1000)]
    public string? vendor_description { get; set; }

    [Required]
    public int address_id { get; set; }

    [Required]
    [MaxLength(25)]
    public string phone { get; set; }

    [MaxLength(25)]
    public string? fax { get; set; }

    [MaxLength(75)]
    public string? general_email { get; set; }

    [MaxLength(250)]
    public string? website { get; set; }

    [MaxLength(50)]
    [Required]
    public string category { get; set; }

    public bool is_critial_vendor { get; set; } = false;

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    public DateTime? approved_on { get; set; }
    public int? approved_by { get; set; }
    public DateTime? audit_on { get; set; }
    public int? audit_by { get; set; }
    public DateTime? retired_on { get; set; }
    public int? retired_by { get; set; }

    [NotMapped]
    public List<Product> products { get; set; } = new List<Product>();
}
