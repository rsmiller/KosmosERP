using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.APInvoiceLine.Command.Delete;

public class APInvoiceLineDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
