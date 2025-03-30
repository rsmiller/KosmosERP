using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Country.Command.Find;

public class CountryFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
