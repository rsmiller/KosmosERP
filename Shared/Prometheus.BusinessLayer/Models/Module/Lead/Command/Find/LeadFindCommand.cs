using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Lead.Command.Find;

public class LeadFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
