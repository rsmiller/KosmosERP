using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Lead.Command.Delete;

public class LeadDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
