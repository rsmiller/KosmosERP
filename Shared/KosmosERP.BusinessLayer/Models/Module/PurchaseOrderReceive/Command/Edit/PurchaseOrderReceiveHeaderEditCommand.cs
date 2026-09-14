using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.PurchaseOrderReceive.Command.Edit;

public class PurchaseOrderReceiveHeaderEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? purchase_order_id { get; set; }
    public bool? is_complete { get; set; }
    public bool? is_canceled { get; set; }
    public string? canceled_reason { get; set; }

    public List<PurchaseOrderReceiveLineEditCommand> received_lines { get; set; } = new List<PurchaseOrderReceiveLineEditCommand>();
}
