using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Country.Command.Create;

public class CountryCreateCommand : DataCommand
{
    [Required]
    public required string country_name { get; set; }
    [Required]
    public required string iso3 { get; set; }
    public string? phonecode { get; set; }
    public string? currency { get; set; }
    public string? currency_symbol { get; set; }
    public string? region { get; set; }
}
