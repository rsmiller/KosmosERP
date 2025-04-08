using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Delete;

public class PurchaseOrderReceiveUploadDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
