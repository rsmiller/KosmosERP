using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Country.Command.Delete;

public class CountryDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
