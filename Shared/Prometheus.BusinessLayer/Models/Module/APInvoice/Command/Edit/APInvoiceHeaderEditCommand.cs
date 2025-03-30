using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Edit;

public class APInvoiceHeaderEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
