using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Transaction.Command.Find;

public class TransactionFindCommand : DataCommand
{
    public string? wildcard { get; set; }
    public int? product_id { get; set; }
    public int? object_reference_id { get; set; }
}
