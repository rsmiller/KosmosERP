using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Find;

public class APInvoiceHeaderFindCommand : DataCommand
{
    public string? wildcard { get; set; }
}
