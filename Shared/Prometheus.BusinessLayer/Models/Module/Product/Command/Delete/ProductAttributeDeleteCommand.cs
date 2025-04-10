using Prometheus.Models;
using System.ComponentModel.DataAnnotations;
namespace Prometheus.BusinessLayer.Models.Module.Product.Command.Delete;

public class ProductAttributeDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
