using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.BOM.Command.Delete;

public class BOMDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
