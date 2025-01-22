using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.State.Command.Edit;

public class StateEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? country_id { get; set; }
    public string? state_name { get; set; }
    public string? iso2 { get; set; }
}
