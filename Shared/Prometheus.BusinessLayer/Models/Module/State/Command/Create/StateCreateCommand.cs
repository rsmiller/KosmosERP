using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.State.Command.Create;

public class StateCreateCommand : DataCommand
{
    [Required]
    public int country_id { get; set; }
    [Required]
    public required string state_name { get; set; }
    [Required]
    public required string iso2 { get; set; }
}
