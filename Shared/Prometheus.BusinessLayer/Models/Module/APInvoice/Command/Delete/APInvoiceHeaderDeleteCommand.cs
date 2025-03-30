using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Delete;

public class APInvoiceHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
