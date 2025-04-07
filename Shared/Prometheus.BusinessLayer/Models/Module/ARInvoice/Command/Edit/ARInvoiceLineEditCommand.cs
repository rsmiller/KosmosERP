using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Edit;

public class ARInvoiceLineEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
