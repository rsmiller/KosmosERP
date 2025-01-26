using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Country.Command.Find;

public class CountryFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
