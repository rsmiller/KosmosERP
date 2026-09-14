using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models;

public class Address : BaseDatabaseModel
{
    [Required]
    [MaxLength(250)]
    public string street_address1 { get; set; }

    [MaxLength(250)]
    public string? street_address2 { get; set; }

    [Required]
    [MaxLength(75)]
    public string city { get; set; }

    [Required]
    [MaxLength(4)]
    public string state { get; set; }

    [Required]
    [MaxLength(10)]
    public string postal_code { get; set; }

    [Required]
    [MaxLength(4)]
    public string country { get; set; }

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}
