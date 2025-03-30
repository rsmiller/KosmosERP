using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Product.Command.Delete;

public class ProductDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
