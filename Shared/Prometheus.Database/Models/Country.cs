using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class Country : BaseDatabaseModel
{
    [Required]
    [MaxLength(50)]
    public string country_name { get; set; }

    [Required]
    [MaxLength(4)]
    public string iso3 { get; set; }

    [MaxLength(10)]
    public string? phonecode { get; set; }

    [MaxLength(25)]
    public string? currency { get; set; }

    [MaxLength(5)]
    public string? currency_symbol { get; set; }

    [MaxLength(50)]
    public string? region { get; set; }

    [NotMapped]
    public List<State> states { get; set; } = new List<State>();
}
