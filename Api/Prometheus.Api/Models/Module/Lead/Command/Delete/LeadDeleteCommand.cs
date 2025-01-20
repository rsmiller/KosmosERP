using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Lead.Command.Delete;

public class LeadDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
