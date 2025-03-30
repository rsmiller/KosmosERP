using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Country.Command.Edit;

public class CountryEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public string? country_name { get; set; }
    public string? iso3 { get; set; }
    public string? phonecode { get; set; }
    public string? currency { get; set; }
    public string? currency_symbol { get; set; }
    public string? region { get; set; }
}
