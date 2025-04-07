using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Find;

public class ARInvoiceHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
