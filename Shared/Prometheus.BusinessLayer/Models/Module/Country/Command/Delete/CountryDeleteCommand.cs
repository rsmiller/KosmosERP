using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Country.Command.Delete;

public class CountryDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
