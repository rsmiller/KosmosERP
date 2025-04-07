using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Delete;

public class ARInvoiceHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
