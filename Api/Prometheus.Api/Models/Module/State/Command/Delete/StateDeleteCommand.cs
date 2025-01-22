using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.State.Command.Delete;

public class StateDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
