using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Lead.Command.Find;

public class LeadFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
