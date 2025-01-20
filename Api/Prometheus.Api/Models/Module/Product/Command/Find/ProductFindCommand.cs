using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Product.Command.Find;

public class ProductFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
