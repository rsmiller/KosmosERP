using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.Product.Command.Find;

public class ProductFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
