using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Customer.Command.Find;

public class CustomerFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
